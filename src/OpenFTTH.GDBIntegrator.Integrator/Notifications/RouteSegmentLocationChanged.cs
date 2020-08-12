using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentLocationChanged : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
        public bool IsLastEventInCmd { get; set; }
    }

    public class RouteSegmentLocationChangedHandler : INotificationHandler<RouteSegmentLocationChanged>
    {
        private readonly ILogger<RouteSegmentLocationChangedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteSegmentLocationChangedHandler(
            ILogger<RouteSegmentLocationChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteSegmentLocationChanged request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentLocationChanged)} with mrid '{request.RouteSegment.Mrid}' to producer");

            var routeSegmentGeometryModifiedEvent = new RouteSegmentGeometryModified
                (
                    nameof(RouteSegmentGeometryModified),
                    Guid.NewGuid(),
                    nameof(RouteSegmentLocationChanged),
                    request.CmdId,
                    request.IsLastEventInCmd,
                    request.RouteSegment.WorkTaskMrid,
                    request.RouteSegment.Username,
                    request.RouteSegment?.ApplicationName,
                    request.RouteSegment?.ApplicationInfo,
                    request.RouteSegment.Mrid,
                    request.RouteSegment.GetGeoJsonCoordinate()
                );

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, routeSegmentGeometryModifiedEvent);
        }
    }
}
