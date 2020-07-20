using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeAdded : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid EventId { get; set; }
    }

    public class RouteNodeAddedHandler : INotificationHandler<RouteNodeAdded>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteNodeAddedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }


        public async Task Handle(RouteNodeAdded request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteNodeAdded)} with mrid '{request.RouteNode.Mrid}' to producer");

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new EventMessages.RouteNodeAdded(request.EventId, request.RouteNode.Mrid, request.RouteNode.GetGeoJsonCoordinate()));
        }
    }
}
