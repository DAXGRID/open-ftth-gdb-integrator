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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly IModifiedGeomitriesStore _modifiedGeomitriesStore;

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
            IModifiedGeomitriesStore modifiedGeomitriesStore)
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
            _modifiedGeomitriesStore = modifiedGeomitriesStore;
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
                    await HandleInvalidMessage((InvalidMessage)request.UpdateMessage);

                var editOperationOccuredEvent = CreateEditOperationOccuredEvent(request.UpdateMessage);

                if (_eventStore.Get().Count() > 0)
                    await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, editOperationOccuredEvent);

                await _geoDatabase.Commit();

                if (_eventStore.Get().Count() > 0 && _applicationSettings.SendGeographicalAreaUpdatedNotification)
                    await _mediator.Publish(new GeographicalAreaUpdated() { RouteNodes = _modifiedGeomitriesStore.GetRouteNodes(), RouteSegment = _modifiedGeomitriesStore.GetRouteSegments() });
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.ToString()}: Rolling back geodatabase transactions");
                await _geoDatabase.RollbackTransaction();
            }
            finally
            {
                _eventStore.Clear();
                _pool.Release();
            }

            return await Task.FromResult(new Unit());
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
                var routeNodeUpdatedEvents = await _routeNodeEventFactory.CreateUpdatedEvent(routeNodeMessage.Before, routeNodeMessage.After);
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
                var routeSegmentUpdatedEvents = await _routeSegmentEventFactory.CreateUpdatedEvent(routeSegmentMessage.Before, routeSegmentMessage.After);
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

        private async Task HandleInvalidMessage(InvalidMessage invalidMessage)
        {
            if (invalidMessage.Message is RouteSegmentMessage)
            {
                var rollbackMessage = (RouteSegmentMessage)invalidMessage.Message;
                await _mediator.Publish(new RollbackInvalidRouteSegment(rollbackMessage.Before));
            }
            if (invalidMessage.Message is RouteNodeMessage)
            {
                var rollbackMessage = (RouteNodeMessage)invalidMessage.Message;
                await _mediator.Publish(new RollbackInvalidRouteNode(rollbackMessage.Before));
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
