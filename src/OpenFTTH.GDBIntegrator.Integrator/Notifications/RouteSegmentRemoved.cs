using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsLastEventInCmd { get; set; }
    }

    public class RouteSegmentRemovedHandler : INotificationHandler<RouteSegmentRemoved>
    {
        private readonly ILogger<RouteSegmentRemovedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IEventStore _eventStore;

        public RouteSegmentRemovedHandler(
            ILogger<RouteSegmentRemovedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentRemoved request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteSegmentRemoved)} with mrid '{request.RouteSegment.Mrid}' to producer");

            var routeSegmentRemoved = new Events.RouteNetwork.RouteSegmentRemoved
                (
                    nameof(Events.RouteNetwork.RouteSegmentRemoved),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    request.CmdType,
                    request.CmdId,
                    request.RouteSegment.Mrid,
                    request.ReplacedBySegments.ToArray(),
                    request.IsLastEventInCmd,
                    request.RouteSegment.WorkTaskMrid,
                    request.RouteSegment.Username,
                    request.RouteSegment?.ApplicationName,
                    request.RouteSegment?.ApplicationInfo
                );

            _eventStore.Insert(routeSegmentRemoved);
            await Task.CompletedTask;
        }
    }
}
