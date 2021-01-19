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
            _logger.LogInformation(
                $"Starting {nameof(RouteNodeInfoUpdatedHandler)}");

            var nodeModifiedEvent = _modifiedEventFactory.CreateRouteNodeInfoModified(request.RouteNode);

            var command = new RouteNetworkCommand(
                nameof(RouteNodeInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { nodeModifiedEvent });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
