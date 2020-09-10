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
    public class RouteNodeAdded : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
        public string CmdType { get; set; }
        public bool IsLastEventInCmd { get; set; }
    }

    public class RouteNodeAddedHandler : INotificationHandler<RouteNodeAdded>
    {
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IEventStore _eventStore;

        public RouteNodeAddedHandler(
            ILogger<RouteNodeAddedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeAdded request, CancellationToken token)
        {
            _logger.LogInformation($"Sending {nameof(RouteNodeAdded)} with mrid '{request.RouteNode.Mrid}' to producer");

            var routeNodeAddedEvent = new Events.RouteNetwork.RouteNodeAdded
                (
                    nameof(Events.RouteNetwork.RouteNodeAdded),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    string.IsNullOrEmpty(request.CmdType) ? nameof(RouteNodeAdded) : request.CmdType,
                    request.CmdId,
                    request.IsLastEventInCmd,
                    request.RouteNode.WorkTaskMrid,
                    request.RouteNode.Username,
                    request.RouteNode?.ApplicationName,
                    request.RouteNode?.ApplicationInfo,
                    request.RouteNode?.NamingInfo,
                    request.RouteNode?.LifeCycleInfo,
                    request.RouteNode?.MappingInfo,
                    request.RouteNode?.SafetyInfo,
                    request.RouteNode.Mrid,
                    request.RouteNode.GetGeoJsonCoordinate(),
                    request.RouteNode?.RouteNodeInfo
                );

            _eventStore.Insert(routeNodeAddedEvent);
            await Task.CompletedTask;
        }
    }
}
