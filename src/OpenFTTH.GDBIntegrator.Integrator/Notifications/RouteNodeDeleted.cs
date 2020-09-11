using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeDeleted : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteNodeDeletedHandler : INotificationHandler<RouteNodeDeleted>
    {
        private readonly IEventStore _eventStore;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;

        public RouteNodeDeletedHandler(
            IEventStore eventStore,
            IRouteNodeEventFactory routeNodeEventFactory)
        {
            _eventStore = eventStore;
            _routeNodeEventFactory = routeNodeEventFactory;
        }

        public async Task Handle(RouteNodeDeleted request, CancellationToken token)
        {
            var routeNodeMarkedForDeletionEvent = _routeNodeEventFactory.CreateMarkedForDeletion(request.RouteNode);

            var markedForDeletionCommand = new RouteNetworkCommand(nameof(RouteNodeMarkedForDeletion), request.CmdId, new List<RouteNetworkEvent> { routeNodeMarkedForDeletionEvent }.ToArray());

            _eventStore.Insert(markedForDeletionCommand);
            await Task.CompletedTask;
        }
    }
}
