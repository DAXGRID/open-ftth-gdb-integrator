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
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentDeleted : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteSegmentDeletedHandler : INotificationHandler<RouteSegmentDeleted>
    {
        private readonly IEventStore _eventStore;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;

        public RouteSegmentDeletedHandler(
            IEventStore eventStore,
            IRouteSegmentEventFactory routeSegmentEventFactory)
        {
            _eventStore = eventStore;
            _routeSegmentEventFactory = routeSegmentEventFactory;
        }

        public async Task Handle(RouteSegmentDeleted request, CancellationToken token)
        {
            var routeSegmentMarkedForDeletionEvent = _routeSegmentEventFactory.CreateMarkedForDeletion(request.RouteSegment);

            var routeSegmentDeletedCommand = new RouteNetworkCommand(nameof(RouteSegmentDeleted), request.CmdId, new List<RouteNetworkEvent> { routeSegmentMarkedForDeletionEvent }.ToArray());

            _eventStore.Insert(routeSegmentDeletedCommand);

            await Task.CompletedTask;
        }
    }
}
