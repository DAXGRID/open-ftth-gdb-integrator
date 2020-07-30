using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeDeleted : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid EventId { get; set; }
    }

    public class RouteNodeDeletedHandler : INotificationHandler<RouteNodeDeleted>
    {
        private readonly ILogger<RouteNodeDeletedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteNodeDeletedHandler(
            ILogger<RouteNodeDeletedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteNodeDeleted request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteNodeDeleted)} with mrid '{request.RouteNode.Mrid}' to producer");

            await _producer.Produce(
                _kafkaSettings.EventRouteNetworkTopicName,
                new EventMessages.RouteNodeMarkedForDeletion
                (
                    request.EventId,
                    request.RouteNode.Mrid,
                    nameof(RouteSegmentDeleted)
                ));
        }
    }
}
