using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewLonelyRouteSegmentCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewLonelyRouteSegmentCommandHandler : AsyncRequestHandler<NewLonelyRouteSegmentCommand>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<NewLonelyRouteSegmentCommandHandler> _logger;

        public NewLonelyRouteSegmentCommandHandler(IGeoDatabase geoDatabase, ILogger<NewLonelyRouteSegmentCommandHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        protected override async Task Handle(NewLonelyRouteSegmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.UtcNow} UTC: Starting - New lonely route segment.\n");

            var routeSegment = request.RouteSegment;

            await _geoDatabase.InsertRouteNode(routeSegment.FindStartNode());
            await _geoDatabase.InsertRouteNode(routeSegment.FindEndNode());

            _logger.LogInformation($"{DateTime.UtcNow} UTC: Finished - New lonely route segment.\n");
        }
    }
}
