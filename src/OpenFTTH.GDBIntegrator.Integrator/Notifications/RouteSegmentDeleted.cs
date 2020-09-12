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
    public class RouteSegmentDeleted : INotification
    {
        public RouteSegment RouteSegment { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteSegmentDeletedHandler : INotificationHandler<RouteSegmentDeleted>
    {
        private readonly IEventStore _eventStore;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly ILogger<RouteSegmentDeletedHandler> _logger;

        public RouteSegmentDeletedHandler(
            IEventStore eventStore,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            ILogger<RouteSegmentDeletedHandler> logger)
        {
            _eventStore = eventStore;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _logger = logger;
        }

        public async Task Handle(RouteSegmentDeleted request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(RouteSegmentDeletedHandler)}");

            var routeSegmentMarkedForDeletionEvent = _routeSegmentEventFactory.CreateMarkedForDeletion(request.RouteSegment);

            var routeSegmentDeletedCommand = new RouteNetworkCommand(nameof(RouteSegmentDeleted), request.CmdId, new List<RouteNetworkEvent> { routeSegmentMarkedForDeletionEvent }.ToArray());

            _eventStore.Insert(routeSegmentDeletedCommand);

            await Task.CompletedTask;
        }
    }
}
