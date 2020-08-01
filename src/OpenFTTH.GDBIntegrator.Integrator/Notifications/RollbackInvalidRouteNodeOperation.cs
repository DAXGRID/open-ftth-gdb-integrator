using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RollbackInvalidRouteNodeOperation : INotification
    {
        public RouteNode RollbackToNode { get; }

        public RollbackInvalidRouteNodeOperation(RouteNode rollbackToNode)
        {
            RollbackToNode = rollbackToNode;
        }
    }

    public class RollbackInvalidRouteNodeOperationHandler : INotificationHandler<RollbackInvalidRouteNodeOperation>
    {
        private readonly ILogger<RollbackInvalidRouteNodeOperationHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;

        public RollbackInvalidRouteNodeOperationHandler(ILogger<RollbackInvalidRouteNodeOperationHandler> logger, IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(RollbackInvalidRouteNodeOperation request, CancellationToken token)
        {
            _logger.LogWarning($"Rollbacks invalid route node operation for RouteNode with id: '{request.RollbackToNode.Mrid}'");
            await _geoDatabase.UpdateRouteNode(request.RollbackToNode);
        }
    }
}
