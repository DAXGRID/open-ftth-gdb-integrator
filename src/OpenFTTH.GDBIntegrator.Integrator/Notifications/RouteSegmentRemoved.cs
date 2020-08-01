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
    public class RouteSegmentRemoved : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
        public IEnumerable<Guid> ReplacedBySegments { get; set; }
        public string CmdType { get; set; }
    }

    public class RouteSegmentRemovedHandler : INotificationHandler<RouteSegmentRemoved>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteSegmentRemovedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteSegmentRemoved request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentRemoved)} with mrid '{request.RouteSegment.Mrid}' to producer");

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                    new EventMessages.RouteSegmentRemoved(
                                        request.CmdId,
                                        request.RouteSegment.Mrid,
                                        request.ReplacedBySegments,
                                        request.CmdType
                                        ));
        }
    }
}
