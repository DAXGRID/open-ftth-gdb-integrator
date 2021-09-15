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
        public RouteSegment FailedSegment { get; }
        public string Message { get; }

        public RollbackInvalidRouteSegment(RouteSegment rollbackToSegment)
        {
            FailedSegment = rollbackToSegment;
        }

        public RollbackInvalidRouteSegment(RouteSegment failedSegment, string message)
        {
            FailedSegment = failedSegment;
            Message = message;
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
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteSegment)} with id: '{request.FailedSegment.Mrid}'. {request.Message}");
            var rollbackSegment = await _geoDatabase.GetRouteSegmentShadowTable(request.FailedSegment.Mrid);
            await _geoDatabase.UpdateRouteSegment(rollbackSegment);
        }
    }
}
