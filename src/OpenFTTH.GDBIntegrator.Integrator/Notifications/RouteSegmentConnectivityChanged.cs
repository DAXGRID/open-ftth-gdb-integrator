using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentConnectivityChanged : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteSegmentConnectivityChangedHandler : INotificationHandler<RouteSegmentConnectivityChanged>
    {
        private readonly ILogger<RouteSegmentConnectivityChangedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;
        private readonly IGeoDatabase _geoDatabase;

        public RouteSegmentConnectivityChangedHandler(
            ILogger<RouteSegmentConnectivityChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer,
            IGeoDatabase geoDatabase)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(RouteSegmentConnectivityChanged request, CancellationToken token)
        {
           
        }
    }
}
