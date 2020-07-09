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
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow} UTC: Starting - New lonely route segment.\n");

            await _geoDatabase.InsertRouteNode(request.RouteSegment.FindStartNode());
            await _geoDatabase.InsertRouteNode(request.RouteSegment.FindEndNode());

            _logger.LogInformation($"{DateTime.UtcNow} UTC: Finished - New lonely route segment.\n");
        }
    }
}
