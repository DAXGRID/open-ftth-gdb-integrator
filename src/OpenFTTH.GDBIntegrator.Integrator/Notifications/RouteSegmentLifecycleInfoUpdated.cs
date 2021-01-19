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
    public class RouteSegmentLifecycleInfoUpdated : INotification
    {
        public RouteSegment RouteSegment { get; }

        public RouteSegmentLifecycleInfoUpdated(RouteSegment routeSegment)
        {
            RouteSegment = routeSegment;
        }
    }

    public class RouteSegmentLifecycleInfoUpdatedHandler : INotificationHandler<RouteSegmentLifecycleInfoUpdated>
    {
        private readonly ILogger<RouteSegmentLifecycleInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentLifecycleInfoUpdatedHandler(
            ILogger<RouteSegmentLifecycleInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentLifecycleInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteSegmentLifecycleInfoUpdatedHandler)}");

            var lifecycleModifiedEvent = _modifiedEventFactory.CreateLifeCycleInfoModified(request.RouteSegment);

            var command = new RouteNetworkCommand(
                nameof(LifecycleInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { lifecycleModifiedEvent });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
