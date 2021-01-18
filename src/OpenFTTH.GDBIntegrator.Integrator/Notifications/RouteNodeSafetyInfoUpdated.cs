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
    public class RouteNodeSafetyInfoUpdated : INotification
    {
        public RouteNode RouteNode { get; }

        public RouteNodeSafetyInfoUpdated(RouteNode routeNode)
        {
            RouteNode = routeNode;
        }
    }

    public class RouteNodeSafetyInfoUpdatedHandler : INotificationHandler<RouteNodeSafetyInfoUpdated>
    {
        private readonly ILogger<RouteNodeSafetyInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteNodeSafetyInfoUpdatedHandler(
            ILogger<RouteNodeSafetyInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeSafetyInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteNodeSafetyInfoUpdatedHandler)}");

            var safetyInfoModified = _modifiedEventFactory.CreateSafetyInfoModified(request.RouteNode);

            var command = new RouteNetworkCommand(
                nameof(SafetyInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { safetyInfoModified });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
