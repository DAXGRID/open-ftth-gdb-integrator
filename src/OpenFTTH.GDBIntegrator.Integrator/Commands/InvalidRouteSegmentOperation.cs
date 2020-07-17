using System;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class InvalidRouteSegmentOperation : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class InvalidRouteSegmentOperationHandler : IRequestHandler<InvalidRouteSegmentOperation, Unit>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteSegmentOperationHandler> _logger;

        public InvalidRouteSegmentOperationHandler(IGeoDatabase geoDatabase, ILogger<InvalidRouteSegmentOperationHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        public async Task<Unit> Handle(InvalidRouteSegmentOperation request, CancellationToken token)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Deleteting {nameof(RouteSegment)} with mrid '{request.RouteSegment.Mrid}'");
            await _geoDatabase.DeleteRouteSegment(request.RouteSegment.Mrid);

            return await Task.FromResult(new Unit());
        }
    }
}
