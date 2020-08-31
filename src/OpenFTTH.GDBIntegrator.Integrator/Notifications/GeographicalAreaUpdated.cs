using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class GeographicalAreaUpdated : INotification
    {
        public List<RouteNode> RouteNodes { get; set; }
        public List<RouteSegment> RouteSEgment { get; set; }
    }

    public class GeographicalAreaUpdatedHandler : INotificationHandler<GeographicalAreaUpdated>
    {
        private readonly IProducer _producer;
        private readonly ILogger<GeographicalAreaUpdatedHandler> _logger;
        private readonly IEnvelopeFactory _envelopeFactory;
        private readonly KafkaSetting _kafkaSettings;

        public GeographicalAreaUpdatedHandler(
            IProducer producer,
            ILogger<GeographicalAreaUpdatedHandler> logger,
            IEnvelopeFactory envelopeFactory,
            IOptions<KafkaSetting> kafkaSettings)
        {
            _producer = producer;
            _logger = logger;
            _envelopeFactory = envelopeFactory;
            _kafkaSettings = kafkaSettings.Value;
        }

        public async Task Handle(GeographicalAreaUpdated request, CancellationToken token)
        {
            await _producer.Produce("", null);
        }
    }
}
