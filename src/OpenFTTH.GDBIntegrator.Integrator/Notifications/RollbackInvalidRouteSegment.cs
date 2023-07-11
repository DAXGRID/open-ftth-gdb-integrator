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
    public class RollbackInvalidRouteSegment : INotification
    {
        public RouteSegment RollbackToSegment { get; private set; }
        public string Message { get; private set; }
        public string ErrorCode { get; private set; }
        public string Username { get; private set; }

        public RollbackInvalidRouteSegment(
            RouteSegment rollbackToSegment,
            string message,
            string errorCode,
            string username)
        {
            RollbackToSegment = rollbackToSegment;
            Message = message;
            ErrorCode = errorCode;
            Username = username;
        }
    }

    public class RollbackInvalidRouteSegmentHandler : INotificationHandler<RollbackInvalidRouteSegment>
    {
        private readonly ILogger<RollbackInvalidRouteSegmentHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly INotificationClient _notificationClient;

        public RollbackInvalidRouteSegmentHandler(
            ILogger<RollbackInvalidRouteSegmentHandler> logger,
            IGeoDatabase geoDatabase,
            INotificationClient notificationClient)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
            _notificationClient = notificationClient;
        }

        public async Task Handle(RollbackInvalidRouteSegment request, CancellationToken token)
        {
            _logger.LogWarning($"Rollbacks invalid {nameof(RouteSegment)} with id: '{request.RollbackToSegment.Mrid}'. {request.Message ?? request.ErrorCode}");

            await _geoDatabase.UpdateRouteSegment(request.RollbackToSegment);

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
