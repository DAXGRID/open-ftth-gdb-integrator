using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Validate;
using OpenFTTH.GDBIntegrator.Integrator.WorkTask;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GeoDatabaseUpdated : IRequest
    {
        public object UpdateMessage { get; set; }
    }

    public class GeoDatabaseUpdatedHandler : IRequestHandler<GeoDatabaseUpdated, Unit>
    {
        // This is the global ID for the RouteNetwork event stream. Should not be changed.
        private readonly Guid GLOBAL_STREAM_ID = Guid.Parse("70554b8a-a572-4ab6-b837-19681ed83d35");

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
        private readonly IWorkTaskService _workTaskService;
        private readonly IEventIdStore _eventIdStore;

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
            IValidationService validationService,
            IWorkTaskService workTaskService,
            IEventIdStore eventIdStore)
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
            _workTaskService = workTaskService;
            _eventIdStore = eventIdStore;
        }

        public async Task<Unit> Handle(GeoDatabaseUpdated request, CancellationToken token)
        {

            var eventId = request.UpdateMessage switch
                 {
                    RouteNodeMessage msg => msg.EventId,
                    RouteSegmentMessage msg => msg.EventId,
                    InvalidMessage msg => msg.EventId,
                    _ => throw new ArgumentException(
                        "Could not handle type of '{typeof(request.UpdateMessage)}'.")
                };

            if (_eventIdStore.GetEventIds().Contains(eventId))
            {
                _logger.LogWarning("{EventId} has already been processed.", eventId);
                return await Task.FromResult(new Unit());
            }

            try
            {
                _eventStore.Clear();

                await _geoDatabase.BeginTransaction();

                if (request.UpdateMessage is RouteNodeMessage)
                    await HandleRouteNode((RouteNodeMessage)request.UpdateMessage);
                else if (request.UpdateMessage is RouteSegmentMessage)
                    await HandleRouteSegment((RouteSegmentMessage)request.UpdateMessage);
                else if (request.UpdateMessage is InvalidMessage)
                {
                    var message = (InvalidMessage)request.UpdateMessage;
                    if (message.Delete)
                    {
                        await _geoDatabase.RollbackTransaction();
                        await _geoDatabase.BeginTransaction();

                        // We only do this in very special cases where we cannot rollback
                        await MarkToBeDeleted(
                            (request.UpdateMessage as InvalidMessage).Message,
                            "Message is invalid and we cannot rollback so we mark it to be deleted.");
                        await _geoDatabase.Commit();

                        // We send an updated event out, to notify that something has been rolled back to refresh GIS.
                        await SendGeographicalAreaUpdatedError(request);

                        return await Task.FromResult(new Unit());
                    }
                    else
                    {
                        await _geoDatabase.RollbackTransaction();
                        await _geoDatabase.BeginTransaction();

                        await RollbackOrDelete((request.UpdateMessage as InvalidMessage).Message, "Message is invalid so we rollback or delete.");
                        await _geoDatabase.Commit();

                        // We send an updated event out, to notify that something has been rolled back to refresh GIS.
                        await SendGeographicalAreaUpdatedError(request);

                        return await Task.FromResult(new Unit());
                    }
                }

                if (_eventStore.Get().Count() > 0)
                {
                    var username = GetUsername(request.UpdateMessage);
                    if (String.IsNullOrWhiteSpace(username))
                    {
                        throw new InvalidOperationException("The update message is missing username.");
                    }

                    var workTaskMrId = await GetUserWorkTaskMrId(username);

                    // We update the work task ids on the newly digitized network elements.
                    await UpdateWorkTaskIdOnNewlyDigitized(workTaskMrId);

                    var editOperationOccuredEvent = CreateEditOperationOccuredEvent(
                        workTaskMrId,
                        username,
                        eventId
                    );

                    if (IsOperationEditEventValid(editOperationOccuredEvent))
                    {
                        await _producer.Produce(GLOBAL_STREAM_ID, editOperationOccuredEvent);
                        _eventIdStore.AppendEventId(eventId);
                        await _geoDatabase.Commit();
                    }
                    else
                    {
                        throw new InvalidOperationException("The edit event operation is invalid.");
                    }
                }
                else
                {
                    await _geoDatabase.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}: Rolling back geodatabase transactions.");
                await _geoDatabase.RollbackTransaction();
                await _geoDatabase.BeginTransaction();
                await RollbackOrDelete(request.UpdateMessage, $"Rollback or delete because of exception: {e}");
                await _geoDatabase.Commit();
                _logger.LogInformation($"{nameof(RouteNetworkEditOperationOccuredEvent)} is now rolled rollback.");

                // We send an updated event out, to notify that something has been rolled back to refresh GIS.
                await SendGeographicalAreaUpdatedError(request);
            }
            finally
            {
                if (_modifiedGeometriesStore.GetRouteNodes().Count > 0 || _modifiedGeometriesStore.GetRouteSegments().Count > 0)
                {
                    try
                    {
                        await _mediator.Publish(new GeographicalAreaUpdated
                        {
                            RouteNodes = _modifiedGeometriesStore.GetRouteNodes(),
                            RouteSegment = _modifiedGeometriesStore.GetRouteSegments()
                        });
                    }
                    catch (Exception ex)
                    {
                        // This is not good, but the application can still keep running even
                        // if there are issues with the notification server.
                        // If a notification is not broadcastet the worst thing that can happen
                        // is that a user has to refresh the UI themselves.
                        _logger.LogInformation(
                            $"Could not broadcast {nameof(GeographicalAreaUpdated)}.\n{ex}");
                    }
                }
                _eventStore.Clear();
                _modifiedGeometriesStore.Clear();
                await _geoDatabase.DisposeTransaction();
                await _geoDatabase.DisposeConnection();
            }

            return await Task.FromResult(new Unit());
        }

        // Use this function to send geographicalareaupdated when an error has occured, since we cannot use the modified geometries store.
        private async Task SendGeographicalAreaUpdatedError(GeoDatabaseUpdated request)
        {
            try
            {
                if (request.UpdateMessage is RouteNodeMessage)
                {
                    var updateMessage = ((RouteNodeMessage)request.UpdateMessage);
                    RouteNode routeNode = null;
                    if (updateMessage.After.Coord is not null)
                    {
                        routeNode = updateMessage.After;
                    }
                    else if (updateMessage.Before.Coord is not null)
                    {
                        routeNode = updateMessage.Before;
                    }

                    if (routeNode is not null)
                    {
                        await _mediator.Publish(new GeographicalAreaUpdated
                        {
                            RouteNodes = new List<RouteNode> { routeNode },
                            RouteSegment = new List<RouteSegment>(),
                        });
                    }
                }
                else if (request.UpdateMessage is RouteSegmentMessage)
                {
                    var updateMessage = ((RouteSegmentMessage)request.UpdateMessage);
                    RouteSegment routeSegment = null;
                    if (updateMessage.After.Coord is not null)
                    {
                        routeSegment = updateMessage.After;
                    }
                    else if (updateMessage.Before.Coord is not null)
                    {
                        routeSegment = updateMessage.Before;
                    }

                    if (routeSegment is not null)
                    {
                        await _mediator.Publish(new GeographicalAreaUpdated
                        {
                            RouteNodes = new List<RouteNode>(),
                            RouteSegment = new List<RouteSegment> { routeSegment }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not send out {nameof(GeographicalAreaUpdated)} Exception: {ex}");
            }
        }

        private async Task MarkToBeDeleted(object message, string errorMessage)
        {
            if (message is RouteSegmentMessage)
            {
                var routeSegmentMessage = (RouteSegmentMessage)message;

                // We get this to check if it is deleted it is deleted if it is null
                var shadowTableSegment = await _geoDatabase.GetRouteSegmentShadowTable(routeSegmentMessage.After.Mrid);

                if (!routeSegmentMessage.After.MarkAsDeleted || shadowTableSegment != null)
                {
                    _logger.LogError($"RouteSegement with id {routeSegmentMessage.After.Mrid}, error message: {errorMessage}");
                    await _geoDatabase.MarkDeleteRouteSegment(routeSegmentMessage.After.Mrid);
                }
            }
            else if (message is RouteNodeMessage)
            {
                var routeNodeMessage = (RouteNodeMessage)message;

                // We get this to check if it is deleted it is deleted if it is null
                var shadowTableNode = await _geoDatabase.GetRouteNodeShadowTable(routeNodeMessage.After.Mrid);

                if (!routeNodeMessage.After.MarkAsDeleted || shadowTableNode != null)
                {
                    _logger.LogError($"RouteNode with id {routeNodeMessage.After.Mrid}, error message: {errorMessage}");
                    await _geoDatabase.MarkDeleteRouteNode(routeNodeMessage.After.Mrid);
                }
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
                    var rollbackSegment = await _geoDatabase.GetRouteSegmentShadowTable(rollbackMessage.After.Mrid);

                    if (rollbackSegment is not null)
                    {
                        await _mediator.Publish(new RollbackInvalidRouteSegment(rollbackSegment, errorMessage));
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
                    var rollbackNode = await _geoDatabase.GetRouteNodeShadowTable(rollbackMessage.After.Mrid);
                    if (rollbackNode is not null)
                    {
                        await _mediator.Publish(new RollbackInvalidRouteNode(rollbackNode, errorMessage));
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
                        throw new Exception("Cannot update route node since it has related equipment.");
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
                        throw new Exception("Cannot update route segment since it has related equipment.");
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

        private string GetUsername(object updateMessage)
        {
            var username = "";

            if (updateMessage is RouteSegmentMessage)
            {
                username = ((RouteSegmentMessage)updateMessage).After.Username;
            }
            else if (updateMessage is RouteNodeMessage)
            {
                username = ((RouteNodeMessage)updateMessage).After.Username;
            }
            else
            {
                throw new ApplicationException($"Could not handle type.");
            }

            return username;
        }

        private async Task<Guid> GetUserWorkTaskMrId(string username)
        {
            var workTask = await _workTaskService.GetUserWorkTask(username);
            if (workTask is null)
            {
                throw new ApplicationException($"User {username} does not have a selected work task.");
            }

            return workTask.Id;
        }

        // Updates work task id on newly digitized routenetwork-elements
        // that do not yet have a WorkTaskMrid
        private async Task UpdateWorkTaskIdOnNewlyDigitized(Guid workTaskMrId)
        {
            foreach (var command in _eventStore.Get())
            {
                foreach (var routeNetworkEvent in command.RouteNetworkEvents)
                {
                    switch (routeNetworkEvent)
                    {
                        case RouteNodeAdded routeNodeAdded:
                            var routeNodeSt = await _geoDatabase.GetRouteNodeShadowTable(routeNodeAdded.NodeId);
                            // If the id is empty, we update it to the current work task id.
                            if (routeNodeSt.WorkTaskMrid == Guid.Empty)
                            {
                                routeNodeSt.WorkTaskMrid = workTaskMrId;
                                await _geoDatabase.UpdateRouteNode(routeNodeSt);
                            }
                            break;
                        case RouteSegmentAdded routeSegmentAdded:
                            var routeSegmentSt = await _geoDatabase.GetRouteSegmentShadowTable(routeSegmentAdded.SegmentId);
                            // If the id is empty, we update it to the current work task id.
                            if (routeSegmentSt is not null && routeSegmentSt.WorkTaskMrid == Guid.Empty)
                            {
                                routeSegmentSt.WorkTaskMrid = workTaskMrId;
                                await _geoDatabase.UpdateRouteSegment(routeSegmentSt);
                            }
                            break;
                    }
                }
            }
        }

        private RouteNetworkEditOperationOccuredEvent CreateEditOperationOccuredEvent(
            Guid workTaskMrid,
            string username,
            Guid eventId)
        {
            return new RouteNetworkEditOperationOccuredEvent(
                nameof(RouteNetworkEditOperationOccuredEvent),
                eventId,
                DateTime.UtcNow,
                workTaskMrid,
                username,
                _applicationSettings.ApplicationName,
                String.Empty,
                _eventStore.Get().ToArray());
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
