using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.Events.Geo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class GeographicalAreaUpdated : INotification
    {
        public List<RouteNode> RouteNodes { get; set; }
        public List<RouteSegment> RouteSegment { get; set; }
    }

    public class GeographicalAreaUpdatedHandler : INotificationHandler<GeographicalAreaUpdated>
    {
        private readonly IProducer _producer;
        private readonly ILogger<GeographicalAreaUpdatedHandler> _logger;
        private readonly IEnvelopeFactory _envelopeFactory;
        private readonly KafkaSetting _kafkaSettings;
        private readonly ApplicationSetting _applicationSettings;

        public GeographicalAreaUpdatedHandler(
            IProducer producer,
            ILogger<GeographicalAreaUpdatedHandler> logger,
            IEnvelopeFactory envelopeFactory,
            IOptions<KafkaSetting> kafkaSettings,
            IOptions<ApplicationSetting> applicationSetting)
        {
            _producer = producer;
            _logger = logger;
            _envelopeFactory = envelopeFactory;
            _kafkaSettings = kafkaSettings.Value;
            _applicationSettings = applicationSetting.Value;
        }

        public async Task Handle(GeographicalAreaUpdated request, CancellationToken token)
        {
            var envelope = _envelopeFactory.Create(request.RouteNodes, request.RouteSegment);
            var envelopeInfo = new EnvelopeInfo(envelope.MinX, envelope.MaxX, envelope.MinY, envelope.MaxY);

            var routeNodesIds = request.RouteNodes.Select(x => x.Mrid);
            var routeSegmentIds = request.RouteSegment.Select(x => x.Mrid);

            var idChangeSets = routeNodesIds.Concat(routeSegmentIds);

            var geographicalAreaUpdatedEvent = new ObjectsWithinGeographicalAreaUpdated(
                nameof(ObjectsWithinGeographicalAreaUpdated),
                Guid.NewGuid(),
                DateTime.UtcNow,
                _applicationSettings.ApplicationName,
                string.Empty,
                "RouteNetworkUpdated",
                envelopeInfo,
                null
                );

            await _producer.Produce(_kafkaSettings.EventGeographicalAreaUpdated, geographicalAreaUpdatedEvent);
        }
    }
}
