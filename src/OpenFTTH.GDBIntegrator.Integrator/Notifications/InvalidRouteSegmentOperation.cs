using System;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class InvalidRouteSegmentOperation : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid EventId { get; set; }
    }

    public class InvalidRouteSegmentOperationHandler : INotificationHandler<InvalidRouteSegmentOperation>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteSegmentOperationHandler> _logger;

        public InvalidRouteSegmentOperationHandler(IGeoDatabase geoDatabase, ILogger<InvalidRouteSegmentOperationHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        public async Task Handle(InvalidRouteSegmentOperation request, CancellationToken token)
        {
            _logger.LogError($"{DateTime.UtcNow.ToString("o")}: Deleteting {nameof(RouteSegment)} with mrid '{request.RouteSegment.Mrid}'");
            await _geoDatabase.DeleteRouteSegment(request.RouteSegment.Mrid);
        }
    }
}
