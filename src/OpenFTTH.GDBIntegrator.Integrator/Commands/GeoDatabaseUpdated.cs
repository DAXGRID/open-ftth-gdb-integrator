using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
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
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(GeoDatabaseUpdated request, CancellationToken token)
        {
            try
            {
                _pool.WaitOne();
                if (request.UpdatedEntity is RouteNode)
                {

                    var notificationEvent = await _routeNodeEventFactory.Create((RouteNode)request.UpdatedEntity);
                    await _mediator.Publish(notificationEvent);
                }
                else if (request.UpdatedEntity is RouteSegment)
                {
                    var notificationEvent = await _routeSegmentEventFactory.Create((RouteSegment)request.UpdatedEntity);
                    await _mediator.Publish(notificationEvent);
                }
                _pool.Release();
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                _pool.Release();
            }

            return await Task.FromResult(new Unit());
        }
    }
}
