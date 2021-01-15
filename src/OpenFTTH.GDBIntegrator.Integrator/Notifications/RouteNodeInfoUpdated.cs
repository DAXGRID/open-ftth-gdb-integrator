using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.Events;
using System;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeInfoUpdated : INotification
    {
        public RouteNode RouteNode { get; }

        public RouteNodeInfoUpdated(RouteNode routeNode)
        {
            RouteNode = routeNode;
        }
    }

    public class RouteNodeInfoUpdatedHandler : INotificationHandler<RouteNodeInfoUpdated>
    {
        private readonly ILogger<RouteNodeInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteNodeInfoUpdatedHandler(
            ILogger<RouteNodeInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeInfoUpdated request, CancellationToken token)
        {
            _logger.LogDebug(
                $"Starting {nameof(RouteNodeInfoModified)}");

            var result = _modifiedEventFactory.CreateRouteNodeInfoModified(request.RouteNode);

            var events = new List<DomainEvent>();

            var cmdId = Guid.NewGuid();

            // TODO insert into store event as a command
        }
    }
}
