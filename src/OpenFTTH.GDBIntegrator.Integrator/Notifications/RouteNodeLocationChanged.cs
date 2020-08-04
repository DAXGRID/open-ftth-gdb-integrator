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
    public class RouteNodeLocationChanged : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteNodeLocationChangedHandler : INotificationHandler<RouteNodeLocationChanged>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteNodeLocationChangedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteNodeLocationChanged request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteNodeLocationChanged)} with mrid '{request.RouteNode.Mrid}' to producer");

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new EventMessages.RouteNodeGeometryModified
                                    (
                                        request.CmdId,
                                        request.RouteNode.Mrid,
                                        nameof(RouteNodeLocationChanged),
                                        request.RouteNode.GetGeoJsonCoordinate()
                                    ));
        }
    }
}
