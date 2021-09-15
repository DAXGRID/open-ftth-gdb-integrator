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
        public RouteNode FailedRouteNode { get; }
        public string Message { get; }

        public RollbackInvalidRouteNode(RouteNode rollbackToNode)
        {
            FailedRouteNode = rollbackToNode;
        }

        public RollbackInvalidRouteNode(RouteNode failedRouteNode, string message)
        {
            FailedRouteNode = failedRouteNode;
            Message = message;
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
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteNode)} with id: '{request.FailedRouteNode.Mrid}'. {request.Message}");
            var rollbackRouteNode = await _geoDatabase.GetRouteNodeShadowTable(request.FailedRouteNode.Mrid);
            await _geoDatabase.UpdateRouteNode(rollbackRouteNode);
        }
    }
}
