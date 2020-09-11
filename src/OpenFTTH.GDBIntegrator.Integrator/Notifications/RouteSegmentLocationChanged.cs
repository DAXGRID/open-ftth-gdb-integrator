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

        public RouteSegmentLocationChangedHandler(
            IEventStore eventStore,
            IRouteSegmentEventFactory routeSegmentEventFactory)
        {
            _eventStore = eventStore;
            _routeSegmentEventFactory = routeSegmentEventFactory;
        }

        public async Task Handle(RouteSegmentLocationChanged request, CancellationToken token)
        {
            var geometryModifiedEvent = _routeSegmentEventFactory.CreateGeometryModified(request.RouteSegment);

            var locationChangedCommand = new RouteNetworkCommand(nameof(RouteSegmentLocationChanged), request.CmdId, new List<RouteNetworkEvent> { geometryModifiedEvent }.ToArray());

            _eventStore.Insert(locationChangedCommand);

            await Task.CompletedTask;
        }
    }
}
