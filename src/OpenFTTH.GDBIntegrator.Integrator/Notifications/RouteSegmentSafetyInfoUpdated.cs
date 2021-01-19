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
    public class RouteSegmentSafetyInfoUpdated : INotification
    {
        public RouteSegment RouteSegment { get; }

        public RouteSegmentSafetyInfoUpdated(RouteSegment routeSegment)
        {
            RouteSegment = routeSegment;
        }
    }

    public class RouteSegmentSafetyInfoUpdatedHandler : INotificationHandler<RouteSegmentSafetyInfoUpdated>
    {
        private readonly ILogger<RouteSegmentSafetyInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentSafetyInfoUpdatedHandler(
            ILogger<RouteSegmentSafetyInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentSafetyInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteSegmentSafetyInfoUpdatedHandler)}");

            var safetyInfoModified = _modifiedEventFactory.CreateSafetyInfoModified(request.RouteSegment);

            var command = new RouteNetworkCommand(
                nameof(SafetyInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { safetyInfoModified });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
