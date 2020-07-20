using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.Integrator.Factories;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GeoDatabaseUpdated : IRequest
    {
        public object UpdatedEntity { get; set; }
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

                if (request.UpdatedEntity is RouteNode)
                {
                    var routeNodeEvent = await _routeNodeEventFactory.Create((RouteNode)request.UpdatedEntity);

                    if (!(routeNodeEvent is null))
                        await _mediator.Publish(routeNodeEvent);
                }
                else if (request.UpdatedEntity is RouteSegment)
                {
                    var routeSegmentEvents = await _routeSegmentEventFactory.Create((RouteSegment)request.UpdatedEntity);

                    foreach (var routeSegmentEvent in routeSegmentEvents)
                    {
                        if (!(routeSegmentEvent is null))
                        {
                            await _mediator.Publish(routeSegmentEvent);
                        }
                    }
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
    }
}
