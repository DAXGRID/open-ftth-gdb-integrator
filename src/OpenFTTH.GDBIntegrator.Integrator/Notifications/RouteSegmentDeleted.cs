using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentDeleted : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
        public bool IsLastEventInCmd { get; set; }
        public string CmdType { get; set; }
    }

    public class RouteSegmentDeletedHandler : INotificationHandler<RouteSegmentDeleted>
    {
        private readonly ILogger<RouteSegmentDeletedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;

        public RouteSegmentDeletedHandler(
            ILogger<RouteSegmentDeletedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
        }

        public async Task Handle(RouteSegmentDeleted request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentDeleted)} with mrid '{request.RouteSegment.Mrid}' to producer");

            var routeSegmentMarkedForDeletionEvent = new RouteSegmentMarkedForDeletion(
                nameof(RouteSegmentMarkedForDeletion),
                Guid.NewGuid(),
                DateTime.UtcNow,
                string.IsNullOrEmpty(request.CmdType) ? nameof(RouteSegmentDeleted) : request.CmdType,
                request.CmdId,
                request.IsLastEventInCmd,
                request.RouteSegment.WorkTaskMrid,
                request.RouteSegment.Username,
                request.RouteSegment?.ApplicationName,
                request.RouteSegment?.ApplicationInfo,
                request.RouteSegment.Mrid);

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, routeSegmentMarkedForDeletionEvent);
        }
    }
}
