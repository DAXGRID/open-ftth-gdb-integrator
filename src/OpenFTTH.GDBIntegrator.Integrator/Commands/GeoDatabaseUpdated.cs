using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.Integrator.Queue;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GeoDatabaseUpdated : IRequest
    {
        public object UpdateMessage { get; set; }
    }

    public class GeoDatabaseUpdatedHandler : IRequestHandler<GeoDatabaseUpdated, Unit>
    {
        private static SemaphoreQueue _pool = new SemaphoreQueue(1, 1);
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;

        public GeoDatabaseUpdatedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IMediator mediator,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IRouteNodeEventFactory routeNodeEventFactory)
        {
            _logger = logger;
            _mediator = mediator;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
        }

        public async Task<Unit> Handle(GeoDatabaseUpdated request, CancellationToken token)
        {
            try
            {
                await _pool.WaitAsync();

                if (request.UpdateMessage is RouteNodeMessage)
                    await HandleRouteNode((RouteNodeMessage)request.UpdateMessage);
                else if (request.UpdateMessage is RouteSegmentMessage)
                    await HandleRouteSegment((RouteSegmentMessage)request.UpdateMessage);

                _pool.Release();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
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
                var routeNodeDigitizedEvent = await _routeNodeEventFactory.CreateDigitizedEvent((RouteNode)routeNodeMessage.After);
                if (!(routeNodeDigitizedEvent is null))
                    await _mediator.Publish(routeNodeDigitizedEvent);
            }
            else if (IsNodeUpdated(routeNodeMessage))
            {
                var routeNodeUpdatedEvent = await _routeNodeEventFactory.CreateUpdatedEvent(routeNodeMessage.Before, routeNodeMessage.After);
                if (!(routeNodeUpdatedEvent is null))
                    await _mediator.Publish(routeNodeUpdatedEvent);
            }
            else
            {
                await _mediator.Publish(new InvalidRouteNodeOperation { RouteNode = routeNodeMessage.After, CmdId = Guid.NewGuid() });
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
                await _mediator.Publish(new InvalidRouteSegmentOperation { RouteSegment = routeSegmentMessage.After, CmdId = Guid.NewGuid() });
            }
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
