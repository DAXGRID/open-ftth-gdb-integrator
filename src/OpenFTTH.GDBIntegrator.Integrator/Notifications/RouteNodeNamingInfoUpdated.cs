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
    public class RouteNodeNamingInfoUpdated : INotification
    {
        public RouteNode RouteNode { get; }

        public RouteNodeNamingInfoUpdated(RouteNode routeNode)
        {
            RouteNode = routeNode;
        }
    }

    public class RouteNodeNamingInfoUpdatedHandler : INotificationHandler<RouteNodeNamingInfoUpdated>
    {
        private readonly ILogger<RouteNodeNamingInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteNodeNamingInfoUpdatedHandler(
            ILogger<RouteNodeNamingInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeNamingInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteNodeNamingInfoUpdatedHandler)}");

            var namingInfoModified = _modifiedEventFactory.CreateNamingInfoModified(request.RouteNode);

            var command = new RouteNetworkCommand(
                nameof(NamingInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { namingInfoModified });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
