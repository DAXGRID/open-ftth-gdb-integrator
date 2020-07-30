using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
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
        private static Semaphore _pool = new Semaphore(1, 1);
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
                _pool.WaitOne();

                if (request.UpdateMessage is RouteNodeMessage)
                {
                    await HandleRouteNode((RouteNodeMessage)request.UpdateMessage);
                }
                else if (request.UpdateMessage is RouteSegmentMessage)
                {
                   await HandleRouteSegment((RouteSegmentMessage)request.UpdateMessage);
                }

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
            var routeNodeEvent = await _routeNodeEventFactory.Create((RouteNode)routeNodeMessage.After);

            if (!(routeNodeEvent is null))
                await _mediator.Publish(routeNodeEvent);
        }

        private async Task HandleRouteSegment(RouteSegmentMessage routeSegmentMessage)
        {
            var routeSegmentEvents = await _routeSegmentEventFactory.Create(routeSegmentMessage.After);

            foreach (var routeSegmentEvent in routeSegmentEvents)
            {
                if (!(routeSegmentEvent is null))
                {
                    await _mediator.Publish(routeSegmentEvent);
                }
            }
        }
    }
}
