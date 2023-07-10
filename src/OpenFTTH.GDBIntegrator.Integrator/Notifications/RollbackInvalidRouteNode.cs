using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RollbackInvalidRouteNode : INotification
    {
        public RouteNode RollbackToNode { get; private set; }
        public string Message { get; private set; }
        public string ErrorCode { get; private set; }
        public string Username { get; private set; }

        public RollbackInvalidRouteNode(
            RouteNode rollbackToNode,
            string message,
            string errorCode,
            string username)
        {
            RollbackToNode = rollbackToNode;
            Message = message;
            ErrorCode = errorCode;
            Username = username;
        }
    }

    public class RollbackInvalidRouteNodeHandler : INotificationHandler<RollbackInvalidRouteNode>
    {
        private readonly ILogger<RollbackInvalidRouteNodeHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly INotificationClient _notificationClient;

        public RollbackInvalidRouteNodeHandler(
            ILogger<RollbackInvalidRouteNodeHandler> logger,
            IGeoDatabase geoDatabase,
            INotificationClient notificationClient)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
            _notificationClient = notificationClient;
        }

        public async Task Handle(RollbackInvalidRouteNode request, CancellationToken token)
        {
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteNode)} with id: '{request.RollbackToNode.Mrid}'. {request.Message}");
            await _geoDatabase.UpdateRouteNode(request.RollbackToNode);

            try
            {
                var userErrorOccurred = new UserErrorOccurred(
                    request.ErrorCode,
                    request.Username);

                _notificationClient.Notify(
                    "UserErrorOccurred",
                    JsonConvert.SerializeObject(userErrorOccurred));
            }
            catch (Exception ex)
            {
                // In case something goes wrong here, we just want to log it and do nothing else.
                // The worst thing that happens is that the user is not notified by the error,
                // but it's better than everything retrying again.
                _logger.LogWarning(
                    "Failed to send user error occurred notification, {Exception}.",
                    ex);
            }
        }
    }
}
