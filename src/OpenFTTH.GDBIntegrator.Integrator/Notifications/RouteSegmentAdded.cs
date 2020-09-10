using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Store;
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
        public Guid CmdId { get; set; }
        public string CmdType { get; set; }
        public bool IsLastEventInCmd { get; set; }
    }

    public class RouteSegmentAddedHandler : INotificationHandler<RouteSegmentAdded>
    {
        private readonly ILogger<RouteSegmentAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IEventStore _eventStore;

        public RouteSegmentAddedHandler(
            ILogger<RouteSegmentAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentAdded request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentAdded)} with mrid '{request.RouteSegment.Mrid}' to producer");

            var routeSegmentAddedEvent = new Events.RouteNetwork.RouteSegmentAdded
                (
                    nameof(Events.RouteNetwork.RouteSegmentAdded),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    request.CmdType,
                    request.CmdId,
                    request.IsLastEventInCmd,
                    request.RouteSegment.WorkTaskMrid,
                    request.RouteSegment.Username,
                    request.RouteSegment?.ApplicationName,
                    request.RouteSegment?.ApplicationInfo,
                    request.RouteSegment?.NamingInfo,
                    request.RouteSegment?.LifeCycleInfo,
                    request.RouteSegment?.MappingInfo,
                    request.RouteSegment?.SafetyInfo,
                    request.RouteSegment.Mrid,
                    request.StartRouteNode.Mrid,
                    request.EndRouteNode.Mrid,
                    request.RouteSegment.GetGeoJsonCoordinate(),
                    request.RouteSegment?.RouteSegmentInfo
                );

            _eventStore.Insert(routeSegmentAddedEvent);
            await Task.CompletedTask;
        }
    }
}
