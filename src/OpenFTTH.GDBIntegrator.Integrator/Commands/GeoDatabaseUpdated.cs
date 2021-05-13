using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.Integrator.Queue;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Integrator.Validate;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GeoDatabaseUpdated : IRequest
    {
        public object UpdateMessage { get; set; }
    }

    public class GeoDatabaseUpdatedHandler : IRequestHandler<GeoDatabaseUpdated, Unit>
    {
        private static SemaphoreQueue _pool = new SemaphoreQueue(1, 1);
        private readonly ILogger<GeoDatabaseUpdatedHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IRouteNodeCommandFactory _routeNodeEventFactory;
        private readonly IRouteSegmentCommandFactory _routeSegmentEventFactory;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IEventStore _eventStore;
        private readonly IProducer _producer;
        private readonly KafkaSetting _kafkaSettings;
        private readonly ApplicationSetting _applicationSettings;
        private readonly IModifiedGeometriesStore _modifiedGeometriesStore;
        private readonly IRouteNodeInfoCommandFactory _routeNodeInfoCommandFactory;
        private readonly IRouteSegmentInfoCommandFactory _routeSegmentInfoCommandFactory;
        private readonly IValidationService _validationService;

        public GeoDatabaseUpdatedHandler(
            ILogger<GeoDatabaseUpdatedHandler> logger,
            IMediator mediator,
            IRouteSegmentCommandFactory routeSegmentEventFactory,
            IRouteNodeCommandFactory routeNodeEventFactory,
            IGeoDatabase geoDatabase,
            IEventStore eventStore,
            IProducer producer,
            IOptions<KafkaSetting> kafkaSettings,
            IOptions<ApplicationSetting> applicationSettings,
            IModifiedGeometriesStore modifiedGeometriesStore,
            IRouteNodeInfoCommandFactory routeNodeInfoCommandFactory,
            IRouteSegmentInfoCommandFactory routeSegmentInfoCommandFactory,
            IValidationService validationService)
        {
            _logger = logger;
            _mediator = mediator;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
            _geoDatabase = geoDatabase;
            _eventStore = eventStore;
            _producer = producer;
            _kafkaSettings = kafkaSettings.Value;
            _applicationSettings = applicationSettings.Value;
            _modifiedGeometriesStore = modifiedGeometriesStore;
            _routeNodeInfoCommandFactory = routeNodeInfoCommandFactory;
            _routeSegmentInfoCommandFactory = routeSegmentInfoCommandFactory;
            _validationService = validationService;
        }

        public async Task<Unit> Handle(GeoDatabaseUpdated request, CancellationToken token)
        {
            try
            {
                await _pool.WaitAsync();
                _eventStore.Clear();
                await _geoDatabase.BeginTransaction();

                if (request.UpdateMessage is RouteNodeMessage)
                    await HandleRouteNode((RouteNodeMessage)request.UpdateMessage);
                else if (request.UpdateMessage is RouteSegmentMessage)
                    await HandleRouteSegment((RouteSegmentMessage)request.UpdateMessage);
                else if (request.UpdateMessage is InvalidMessage)
                {
                    var message = (InvalidMessage)request.UpdateMessage;
                    // We only do this in very special cases where we cannot rollback
                    if (message.Delete)
                    {
                        await Delete((request.UpdateMessage as InvalidMessage).Message, "Message is invalid and we cannot rollback.");
                        await _geoDatabase.Commit();
                        return await Task.FromResult(new Unit());
                    }
                    else
                    {
                        await RollbackOrDelete((request.UpdateMessage as InvalidMessage).Message, "Message is invalid so we rollback or delete");
                    }
                }

                var editOperationOccuredEvent = CreateEditOperationOccuredEvent(request.UpdateMessage);

                if (IsOperationEditEventValid(editOperationOccuredEvent))
                {
                    if (_eventStore.Get().Count() > 0)
                        await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, editOperationOccuredEvent);

                    await _geoDatabase.Commit();

                    if (_eventStore.Get().Count() > 0 && _applicationSettings.SendGeographicalAreaUpdatedNotification)
                    {
                        await _mediator.Publish(new GeographicalAreaUpdated
                        {
                            RouteNodes = _modifiedGeometriesStore.GetRouteNodes(),
                            RouteSegment = _modifiedGeometriesStore.GetRouteSegments()
                        });
                    }
                }
                else
                {
                    _logger.LogError($"{nameof(RouteNetworkEditOperationOccuredEvent)} is not valid so we rollback.");
                    await _geoDatabase.RollbackTransaction();
                    await _geoDatabase.BeginTransaction();
                    await RollbackOrDelete(request.UpdateMessage, $"Rollback because {nameof(RouteNetworkEditOperationOccuredEvent)} is not valid.");
                    await _geoDatabase.Commit();
                    _logger.LogInformation($"{nameof(RouteNetworkEditOperationOccuredEvent)} is now rolled rollback.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.ToString()}: Rolling back geodatabase transactions");
                await _geoDatabase.RollbackTransaction();
                await _geoDatabase.BeginTransaction();
                await RollbackOrDelete(request.UpdateMessage, $"Rollback because of exception: {e.Message}");
                await _geoDatabase.Commit();
            }
            finally
            {
                _eventStore.Clear();
                await _geoDatabase.DisposeTransaction();
                await _geoDatabase.DisposeConnection();
                _pool.Release();
            }

            return await Task.FromResult(new Unit());
        }

        private async Task Delete(object message, string errorMessage)
        {
            if (message is RouteSegmentMessage)
            {
                var routeSegmentMessage = (RouteSegmentMessage)message;
                await _mediator.Publish(new InvalidRouteSegmentOperation
                {
                    RouteSegment = routeSegmentMessage.After,
                    Message = errorMessage
                });
            }
            else if (message is RouteNodeMessage)
            {
                var routeNodeMessage = (RouteNodeMessage)message;
                await _mediator.Publish(new InvalidRouteNodeOperation
                {
                    RouteNode = routeNodeMessage.After,
                    Message = errorMessage
                });
            }
            else
            {
                throw new Exception($"Message of type '{message.GetType()}' is not supported.");
            }
        }

        private async Task RollbackOrDelete(object message, string errorMessage)
        {
            if (message is RouteSegmentMessage)
            {
                var rollbackMessage = (RouteSegmentMessage)message;
                if (rollbackMessage.Before != null)
                {
                    await _mediator.Publish(new RollbackInvalidRouteSegment(rollbackMessage.Before, errorMessage));
                }
                else
                {
                    await _mediator.Publish(new InvalidRouteSegmentOperation
                    {
                        RouteSegment = rollbackMessage.After,
                        Message = errorMessage
                    });
                }
            }
            else if (message is RouteNodeMessage)
            {
                var rollbackMessage = (RouteNodeMessage)message;
                if (rollbackMessage.Before != null)
                {
                    await _mediator.Publish(new RollbackInvalidRouteNode(rollbackMessage.Before, errorMessage));
                }
                else
                {
                    await _mediator.Publish(new InvalidRouteNodeOperation
                    {
                        RouteNode = rollbackMessage.After,
                        Message = errorMessage
                    });
                }
            }
            else
            {
                throw new Exception($"Message of type '{message.GetType()}' is not supported.");
            }
        }

        private bool IsOperationEditEventValid(RouteNetworkEditOperationOccuredEvent operationOccuredEvent)
        {
            var existingSplittedCmds = operationOccuredEvent.RouteNetworkCommands
                .Where(x => x.CmdType == nameof(ExistingRouteSegmentSplitted));

            if (existingSplittedCmds.Count() > 0)
            {
                foreach (var splittedCmd in existingSplittedCmds)
                {
                    var removedEvents = splittedCmd.RouteNetworkEvents
                        .Where(x => x.EventType == nameof(Events.RouteNetwork.RouteSegmentRemoved));
                    var routeSegmentAdded = splittedCmd.RouteNetworkEvents
                        .Where(x => x.EventType == nameof(Events.RouteNetwork.RouteSegmentAdded));

                    if (removedEvents.Count() != 1 || routeSegmentAdded.Count() != 2)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task HandleRouteNode(RouteNodeMessage routeNodeMessage)
        {
            if (IsRouteNodeDeleted(routeNodeMessage))
                return;

            if (IsNodeNewlyDigitized(routeNodeMessage))
            {
                var routeNodeDigitizedEvents = await _routeNodeEventFactory.CreateDigitizedEvent((RouteNode)routeNodeMessage.After);

                foreach (var routeNodeDigitizedEvent in routeNodeDigitizedEvents)
                {
                    if (!(routeNodeDigitizedEvent is null))
                        await _mediator.Publish(routeNodeDigitizedEvent);
                }
            }
            else if (IsNodeUpdated(routeNodeMessage))
            {
                var modifiedEvents = await _routeNodeInfoCommandFactory
                    .Create(routeNodeMessage.Before, routeNodeMessage.After);

                var routeNodeUpdatedEvents = await _routeNodeEventFactory
                    .CreateUpdatedEvent(routeNodeMessage.Before, routeNodeMessage.After);

                var possibleIllegalOperation = routeNodeUpdatedEvents.Any(x => x.GetType() == typeof(RouteNodeDeleted));
                if (possibleIllegalOperation)
                {
                    var hasRelatedEquipment = await _validationService.HasRelatedEquipment(routeNodeMessage.After.Mrid);
                    if (hasRelatedEquipment)
                    {
                        await _mediator.Publish(new RollbackInvalidRouteNode(routeNodeMessage.Before, "Rollback route node since it has related equipment."));
                        return;
                    }
                }

                foreach (var modifiedEvent in modifiedEvents)
                {
                    if (!(modifiedEvent is null))
                        await _mediator.Publish(modifiedEvent);
                }

                foreach (var routeNodeUpdatedEvent in routeNodeUpdatedEvents)
                {
                    if (!(routeNodeUpdatedEvent is null))
                        await _mediator.Publish(routeNodeUpdatedEvent);
                }
            }
            else
            {
                await _mediator.Publish(new InvalidRouteNodeOperation { RouteNode = routeNodeMessage.After });
            }
        }

        private async Task HandleRouteSegment(RouteSegmentMessage routeSegmentMessage)
        {
            if (IsRouteSegmentedDeleted(routeSegmentMessage))
                return;

            if (IsSegmentNewlyDigitized(routeSegmentMessage))
            {
                var routeSegmentDigitizedEvents = await _routeSegmentEventFactory.CreateDigitizedEvent(routeSegmentMessage.After);
                foreach (var routeSegmentDigitizedEvent in routeSegmentDigitizedEvents)
                {
                    if (!(routeSegmentDigitizedEvent is null))
                        await _mediator.Publish(routeSegmentDigitizedEvent);
                }
            }
            else if (IsSegmentUpdated(routeSegmentMessage))
            {
                var infoModifiedEvents = await _routeSegmentInfoCommandFactory
                    .Create(routeSegmentMessage.Before, routeSegmentMessage.After);

                var routeSegmentUpdatedEvents = await _routeSegmentEventFactory
                    .CreateUpdatedEvent(routeSegmentMessage.Before, routeSegmentMessage.After);

                var possibleIllegalOperation = routeSegmentUpdatedEvents.Any(x => x.GetType() == typeof(RouteSegmentDeleted) || x.GetType() == typeof(RouteSegmentConnectivityChanged));
                if (possibleIllegalOperation)
                {
                    var hasRelatedEquipment = await _validationService.HasRelatedEquipment(routeSegmentMessage.After.Mrid);
                    if (hasRelatedEquipment)
                    {
                        await _mediator.Publish(new RollbackInvalidRouteSegment(routeSegmentMessage.Before, "Rollback route segment since it has related equipment."));
                        return;
                    }
                }

                foreach (var modifiedEvent in infoModifiedEvents)
                {
                    if (!(modifiedEvent is null))
                        await _mediator.Publish(modifiedEvent);
                }

                foreach (var routeSegmentUpdatedEvent in routeSegmentUpdatedEvents)
                {
                    if (!(routeSegmentUpdatedEvent is null))
                        await _mediator.Publish(routeSegmentUpdatedEvent);
                }
            }
            else
            {
                await _mediator.Publish(new InvalidRouteSegmentOperation { RouteSegment = routeSegmentMessage.After });
            }
        }

        private RouteNetworkEditOperationOccuredEvent CreateEditOperationOccuredEvent(object updateMessage)
        {
            Guid? workTaskMrid = null;
            string username = null;

            if (updateMessage is RouteSegmentMessage)
            {
                workTaskMrid = ((RouteSegmentMessage)updateMessage).After.WorkTaskMrid;
                username = ((RouteSegmentMessage)updateMessage).After.Username;
            }
            if (updateMessage is RouteNodeMessage)
            {
                workTaskMrid = ((RouteNodeMessage)updateMessage).After.WorkTaskMrid;
                username = ((RouteNodeMessage)updateMessage).After.Username;
            }

            var editOperationOccuredEvent = new RouteNetworkEditOperationOccuredEvent(
                nameof(RouteNetworkEditOperationOccuredEvent),
                Guid.NewGuid(),
                DateTime.UtcNow,
                workTaskMrid,
                username,
                _applicationSettings.ApplicationName,
                String.Empty,
                _eventStore.Get().ToArray());

            return editOperationOccuredEvent;
        }

        private bool IsRouteSegmentedDeleted(RouteSegmentMessage routeSegmentMessage)
        {
            return routeSegmentMessage.Before is null && routeSegmentMessage.After is null;
        }

        private bool IsRouteNodeDeleted(RouteNodeMessage routeNodeMessage)
        {
            return routeNodeMessage.Before is null && routeNodeMessage.After is null;
        }

        private bool IsNodeNewlyDigitized(RouteNodeMessage routeNodeMessage)
        {
            return routeNodeMessage.Before is null && routeNodeMessage.After.Mrid.ToString() != string.Empty;
        }

        private bool IsSegmentNewlyDigitized(RouteSegmentMessage routeSegmentMessage)
        {
            return routeSegmentMessage.Before is null && routeSegmentMessage.After.Mrid.ToString() != string.Empty;
        }

        private bool IsSegmentUpdated(RouteSegmentMessage routeSegmentMessage)
        {
            return !(routeSegmentMessage.Before is null);
        }

        private bool IsNodeUpdated(RouteNodeMessage routeNodeMessage)
        {
            return !(routeNodeMessage.Before is null);
        }
    }
}
