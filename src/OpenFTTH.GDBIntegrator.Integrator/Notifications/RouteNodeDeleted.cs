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
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<RouteNodeDeletedHandler> _logger;

        public RouteNodeDeletedHandler(
            IEventStore eventStore,
            IRouteNodeEventFactory routeNodeEventFactory,
            ILogger<RouteNodeDeletedHandler> logger)
        {
            _eventStore = eventStore;
            _routeNodeEventFactory = routeNodeEventFactory;
            _logger = logger;
        }

        public async Task Handle(RouteNodeDeleted request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(RouteNodeDeletedHandler)}");

            var routeNodeMarkedForDeletionEvent = _routeNodeEventFactory.CreateMarkedForDeletion(request.RouteNode);

            var markedForDeletionCommand = new RouteNetworkCommand(nameof(RouteNodeMarkedForDeletion), request.CmdId, new List<RouteNetworkEvent> { routeNodeMarkedForDeletionEvent }.ToArray());

            _eventStore.Insert(markedForDeletionCommand);
            await Task.CompletedTask;
        }
    }
}
