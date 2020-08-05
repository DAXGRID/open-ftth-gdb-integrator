using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RollbackInvalidRouteNode : INotification
    {
        public RouteNode RollbackToNode { get; }

        public RollbackInvalidRouteNode(RouteNode rollbackToNode)
        {
            RollbackToNode = rollbackToNode;
        }
    }

    public class RollbackInvalidRouteNodeHandler : INotificationHandler<RollbackInvalidRouteNode>
    {
        private readonly ILogger<RollbackInvalidRouteNodeHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;

        public RollbackInvalidRouteNodeHandler(ILogger<RollbackInvalidRouteNodeHandler> logger, IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(RollbackInvalidRouteNode request, CancellationToken token)
        {
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteNode)} with id: '{request.RollbackToNode.Mrid}'");
            await _geoDatabase.UpdateRouteNode(request.RollbackToNode);
        }
    }
}
