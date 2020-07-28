using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentAdded : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public RouteNode StartRouteNode { get; set; }
        public RouteNode EndRouteNode { get; set; }
        public Guid EventId { get; set; }
        public string CmdType { get; set; }
    }

    public class RouteSegmentAddedHandler : INotificationHandler<RouteSegmentAdded>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteSegmentAddedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteSegmentAdded request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentAdded)} with mrid '{request.RouteSegment.Mrid}' to producer");

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new EventMessages.RouteSegmentAdded(
                                        Guid.NewGuid(),
                                        request.RouteSegment.Mrid,
                                        request.StartRouteNode.Mrid,
                                        request.EndRouteNode.Mrid,
                                        request.RouteSegment.GetGeoJsonCoordinate(),
                                        request.CmdType
                                        ));
        }
    }
}
