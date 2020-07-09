using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentBetweenTwoExistingNodesCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewRouteSegmentBetweenTwoExistingNodesCommandHandler : AsyncRequestHandler<NewRouteSegmentBetweenTwoExistingNodesCommand>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler> _logger;

        public NewRouteSegmentBetweenTwoExistingNodesCommandHandler(IGeoDatabase geoDatabase, ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        protected override async Task Handle(NewRouteSegmentBetweenTwoExistingNodesCommand request, CancellationToken cancellationToken)
        {
            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            _logger.LogInformation($"{DateTime.UtcNow} UTC: Starting - New route segment between two existing nodes.\n");

            _logger.LogInformation($"{DateTime.UtcNow} UTC: Finished - New route segment between two existing nodes.\n");
        }
    }
}
