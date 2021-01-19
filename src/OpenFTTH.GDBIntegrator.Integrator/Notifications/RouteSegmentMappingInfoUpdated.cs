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
    public class RouteSegmentMappingInfoUpdated : INotification
    {
        public RouteSegment RouteSegment { get; }

        public RouteSegmentMappingInfoUpdated(RouteSegment routeSegment)
        {
            RouteSegment = routeSegment;
        }
    }

    public class RouteSegmentMappingInfoUpdatedHandler : INotificationHandler<RouteSegmentMappingInfoUpdated>
    {
        private readonly ILogger<RouteSegmentMappingInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentMappingInfoUpdatedHandler(
            ILogger<RouteSegmentMappingInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentMappingInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteSegmentMappingInfoUpdatedHandler)}");

            var mappingInfo = _modifiedEventFactory.CreateMappingInfoModified(request.RouteSegment);

            var command = new RouteNetworkCommand(
                nameof(MappingInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { mappingInfo });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
