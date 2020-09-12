using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteNodeDigitized : INotification
    {
        public RouteNode RouteNode { get; set; }
    }

    public class NewRouteNodeDigitizedHandler : INotificationHandler<NewRouteNodeDigitized>
    {
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IEventStore _eventStore;
        private readonly ILogger<NewRouteNodeDigitizedHandler> _logger;

        public NewRouteNodeDigitizedHandler(
            IRouteNodeEventFactory routeNodeEventFactory,
            IEventStore eventStore,
            ILogger<NewRouteNodeDigitizedHandler> logger)
        {
            _routeNodeEventFactory = routeNodeEventFactory;
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task Handle(NewRouteNodeDigitized request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(NewRouteNodeDigitizedHandler)}");

            var routeNodeAddedEvent = _routeNodeEventFactory.CreateAdded(request.RouteNode);

            var cmdId = Guid.NewGuid();
            var newRouteNodeDigitizedCommand = new RouteNetworkCommand(nameof(NewRouteNodeDigitized), cmdId, new List<RouteNetworkEvent> { routeNodeAddedEvent }.ToArray());

            _eventStore.Insert(newRouteNodeDigitizedCommand);

            await Task.FromResult(0);
        }
    }
}
