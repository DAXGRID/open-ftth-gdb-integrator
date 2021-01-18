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
    public class RouteSegmentNamingInfoUpdated : INotification
    {
        public RouteSegment RouteSegment { get; }

        public RouteSegmentNamingInfoUpdated(RouteSegment routeSegment)
        {
            RouteSegment = routeSegment;
        }
    }

    public class RouteSegmentNamingInfoUpdatedHandler : INotificationHandler<RouteSegmentNamingInfoUpdated>
    {
        private readonly ILogger<RouteSegmentNamingInfoUpdatedHandler> _logger;
        private readonly IModifiedEventFactory _modifiedEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentNamingInfoUpdatedHandler(
            ILogger<RouteSegmentNamingInfoUpdatedHandler> logger,
            IModifiedEventFactory modifiedEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _modifiedEventFactory = modifiedEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentNamingInfoUpdated request, CancellationToken token)
        {
            _logger.LogInformation(
                $"Starting {nameof(RouteSegmentNamingInfoUpdatedHandler)}");

            var namingInfoModified = _modifiedEventFactory.CreateNamingInfoModified(request.RouteSegment);

            var command = new RouteNetworkCommand(
                nameof(NamingInfoModified),
                Guid.NewGuid(),
                new RouteNetworkEvent[] { namingInfoModified });

            _eventStore.Insert(command);

            await Task.CompletedTask;
        }
    }
}
