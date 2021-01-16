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
using OpenFTTH.Events.Core;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class LifecycleInfoUpdated : INotification
    {
        public RouteNode RouteNode { get; }

        public LifecycleInfoUpdated(RouteNode routeNode)
        {
            RouteNode = routeNode;
        }
    }

    public class LifecycleInfoUpdatedHandler : INotificationHandler<LifecycleInfoUpdated>
    {
        private readonly ILogger<RouteNodeInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public LifecycleInfoUpdatedHandler(
            ILogger<RouteNodeInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(LifecycleInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(LifecycleInfoUpdatedHandler)}");

            var lifecycleModifiedEvent = _modifiedEventFactory.CreateLifeCycleInfoModified(request.RouteNode);

            var command = new RouteNetworkCommand(
                nameof(LifecycleInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { lifecycleModifiedEvent });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
