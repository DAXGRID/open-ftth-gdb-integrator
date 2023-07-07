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
    public class InvalidRouteNodeOperation : INotification
    {
        public RouteNode RouteNode { get; private set; }
        public string Message { get; private set; }
        public string ErrorCode { get; private set; }
        public string Username { get; private set; }

        public InvalidRouteNodeOperation(
            RouteNode routeNode,
            string message,
            string errorCode,
            string username)
        {
            RouteNode = routeNode;
            Message = message;
            ErrorCode = errorCode;
            Username = username;
        }
    }

    public class InvalidRouteNodeOperationHandler : INotificationHandler<InvalidRouteNodeOperation>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteNodeOperationHandler> _logger;
        private readonly INotificationClient _notificationClient;

        public InvalidRouteNodeOperationHandler(
            IGeoDatabase geoDatabase,
            ILogger<InvalidRouteNodeOperationHandler> logger,
            INotificationClient notificationClient)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
            _notificationClient = notificationClient;
        }

        public async Task Handle(InvalidRouteNodeOperation request, CancellationToken token)
        {
            _logger.LogWarning($"Deleteting {nameof(RouteNode)} with mrid '{request.RouteNode.Mrid}'. Because: {request.Message}");
            await _geoDatabase.DeleteRouteNode(request.RouteNode.Mrid);

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
                    "Failed to send user error occurred notification, {Exception}",
                    ex);
            }
        }
    }
}
