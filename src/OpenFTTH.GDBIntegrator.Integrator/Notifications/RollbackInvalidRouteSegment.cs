using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RollbackInvalidRouteSegment : INotification
    {
        public RouteSegment RollbackToSegment { get; }

        public RollbackInvalidRouteSegment(RouteSegment rollbackToSegment)
        {
            RollbackToSegment = rollbackToSegment;
        }
    }

    public class RollbackInvalidRouteSegmentHandler : INotificationHandler<RollbackInvalidRouteSegment>
    {
        private readonly ILogger<RollbackInvalidRouteSegmentHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;

        public RollbackInvalidRouteSegmentHandler(ILogger<RollbackInvalidRouteSegmentHandler> logger, IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(RollbackInvalidRouteSegment request, CancellationToken token)
        {
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteSegment)} with id: '{request.RollbackToSegment.Mrid}'");
            await _geoDatabase.UpdateRouteSegment(request.RollbackToSegment);
        }
    }
}
