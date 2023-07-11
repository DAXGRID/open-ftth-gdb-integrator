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
    public class InvalidRouteSegmentOperation : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; init; }
        public string Username { get; init; }

        public InvalidRouteSegmentOperation(
            RouteSegment routeSegment,
            string message,
            string errorCode,
            string username)
        {
            RouteSegment = routeSegment;
            Message = message;
            ErrorCode = errorCode;
            Username = username;
        }
    }

    public class InvalidRouteSegmentOperationHandler : INotificationHandler<InvalidRouteSegmentOperation>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteSegmentOperationHandler> _logger;
        private readonly INotificationClient _notificationClient;

        public InvalidRouteSegmentOperationHandler(
            IGeoDatabase geoDatabase,
            ILogger<InvalidRouteSegmentOperationHandler> logger,
            INotificationClient notificationClient)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
            _notificationClient = notificationClient;
        }

        public async Task Handle(InvalidRouteSegmentOperation request, CancellationToken token)
        {
            _logger.LogWarning($"Deleteting {nameof(RouteSegment)} with mrid '{request.RouteSegment.Mrid}' - Because: {request.Message}");
            await _geoDatabase.DeleteRouteSegment(request.RouteSegment.Mrid);

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
