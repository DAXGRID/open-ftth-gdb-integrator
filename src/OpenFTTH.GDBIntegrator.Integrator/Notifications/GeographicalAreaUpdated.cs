using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenFTTH.Events.Geo;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class GeographicalAreaUpdated : INotification
    {
        public List<RouteNode> RouteNodes { get; set; }
        public List<RouteSegment> RouteSegment { get; set; }
    }

    public class GeographicalAreaUpdatedHandler : INotificationHandler<GeographicalAreaUpdated>
    {
        private readonly ILogger<GeographicalAreaUpdatedHandler> _logger;
        private readonly IEnvelopeFactory _envelopeFactory;
        private readonly ApplicationSetting _applicationSettings;
        private readonly INotificationClient _notificationClient;

        public GeographicalAreaUpdatedHandler(
            ILogger<GeographicalAreaUpdatedHandler> logger,
            IEnvelopeFactory envelopeFactory,
            IOptions<ApplicationSetting> applicationSetting,
            INotificationClient notificationClient)
        {
            _logger = logger;
            _envelopeFactory = envelopeFactory;
            _applicationSettings = applicationSetting.Value;
            _notificationClient = notificationClient;
        }

        public Task Handle(GeographicalAreaUpdated request, CancellationToken token)
        {
            _logger.LogDebug($"Starting {nameof(GeographicalAreaUpdatedHandler)}");

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
                null);

            _notificationClient.Notify(
                "GeographicalAreaUpdated",
                JsonConvert.SerializeObject(geographicalAreaUpdatedEvent));

            return Task.CompletedTask;
        }
    }
}
