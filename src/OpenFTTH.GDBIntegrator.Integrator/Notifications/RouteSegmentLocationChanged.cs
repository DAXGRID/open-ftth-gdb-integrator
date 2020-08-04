using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Integrator;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentLocationChanged : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteSegmentLocationChangedHandler : INotificationHandler<RouteSegmentLocationChanged>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteSegmentLocationChangedHandler(
            ILogger<RouteNodeAddedHandler> logger,
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

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new EventMessages.RouteSegmentGeometryModified
                                    (
                                        request.CmdId,
                                        request.RouteSegment.Mrid,
                                        nameof(RouteSegmentLocationChanged),
                                        request.RouteSegment.GetGeoJsonCoordinate()
                                    ));
        }
    }
}
