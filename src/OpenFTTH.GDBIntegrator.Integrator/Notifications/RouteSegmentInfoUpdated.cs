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
    public class RouteSegmentInfoUpdated : INotification
    {
        public RouteSegment RouteSegment { get; }

        public RouteSegmentInfoUpdated(RouteSegment routeSegment)
        {
            RouteSegment = routeSegment;
        }
    }

    public class RouteSegmentInfoUpdatedHandler : INotificationHandler<RouteSegmentInfoUpdated>
    {
        private readonly ILogger<RouteSegmentInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentInfoUpdatedHandler(
            ILogger<RouteSegmentInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteSegmentInfoUpdatedHandler)}");

            var segmentModifiedEvent = _modifiedEventFactory.CreateRouteSegmentInfoModified(request.RouteSegment);

            var command = new RouteNetworkCommand(
                nameof(RouteSegmentInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { segmentModifiedEvent });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
