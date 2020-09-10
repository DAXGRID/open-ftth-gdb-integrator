using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeDeleted : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
        public bool IsLastEventInCmd { get; set; }
        public string CmdType { get; set; }
    }

    public class RouteNodeDeletedHandler : INotificationHandler<RouteNodeDeleted>
    {
        private readonly ILogger<RouteNodeDeletedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IEventStore _eventStore;

        public RouteNodeDeletedHandler(
            ILogger<RouteNodeDeletedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeDeleted request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteNodeDeleted)} with mrid '{request.RouteNode.Mrid}' to producer");

            var routeNodeDeletedEvent = new RouteNodeMarkedForDeletion
                (
                    nameof(RouteNodeMarkedForDeletion),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    string.IsNullOrEmpty(request.CmdType) ? nameof(RouteSegmentDeleted) : request.CmdType,
                    request.CmdId,
                    request.IsLastEventInCmd,
                    request.RouteNode.WorkTaskMrid,
                    request.RouteNode.Username,
                    request.RouteNode?.ApplicationName,
                    request.RouteNode?.ApplicationInfo,
                    request.RouteNode.Mrid
                );

            _eventStore.Insert(routeNodeDeletedEvent);
            await Task.CompletedTask;
        }
    }
}
