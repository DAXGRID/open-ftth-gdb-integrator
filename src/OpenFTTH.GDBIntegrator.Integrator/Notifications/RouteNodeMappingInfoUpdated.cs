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
    public class RouteNodeMappingInfoUpdated : INotification
    {
        public RouteNode RouteNode { get; }

        public RouteNodeMappingInfoUpdated(RouteNode routeNode)
        {
            RouteNode = routeNode;
        }
    }

    public class RouteNodeMappingInfoUpdatedHandler : INotificationHandler<RouteNodeMappingInfoUpdated>
    {
        private readonly ILogger<RouteNodeMappingInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteNodeMappingInfoUpdatedHandler(
            ILogger<RouteNodeMappingInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteNodeMappingInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteNodeMappingInfoUpdated)}");

            var mappingInfo = _modifiedEventFactory.CreateMappingInfoModified(request.RouteNode);

            var command = new RouteNetworkCommand(
                nameof(MappingInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { mappingInfo });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
