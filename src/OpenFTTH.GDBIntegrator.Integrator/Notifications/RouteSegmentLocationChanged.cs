using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentLocationChanged : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteSegmentLocationChangedHandler : INotificationHandler<RouteSegmentLocationChanged>
    {
        private readonly IEventStore _eventStore;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly ILogger<RouteSegmentLocationChangedHandler> _logger;

        public RouteSegmentLocationChangedHandler(
            IEventStore eventStore,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            ILogger<RouteSegmentLocationChangedHandler> logger)
        {
            _eventStore = eventStore;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _logger = logger;
        }

        public async Task Handle(RouteSegmentLocationChanged request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(RouteSegmentLocationChangedHandler)}");

            var geometryModifiedEvent = _routeSegmentEventFactory.CreateGeometryModified(request.RouteSegment);

            var locationChangedCommand = new RouteNetworkCommand(nameof(RouteSegmentLocationChanged), request.CmdId, new List<RouteNetworkEvent> { geometryModifiedEvent }.ToArray());

            _eventStore.Insert(locationChangedCommand);

            await Task.CompletedTask;
        }
    }
}
