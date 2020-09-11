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

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteNodeDigitized : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid CmdId { get; set; }
    }

    public class NewRouteNodeDigitizedHandler : INotificationHandler<NewRouteNodeDigitized>
    {
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IEventStore _eventStore;

        public NewRouteNodeDigitizedHandler(
            IRouteNodeEventFactory routeNodeEventFactory,
            IEventStore eventStore)
        {
            _routeNodeEventFactory = routeNodeEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(NewRouteNodeDigitized request, CancellationToken token)
        {
            var routeNodeAddedEvent = _routeNodeEventFactory.CreateAdded(request.RouteNode);

            var newRouteNodeDigitizedCommand = new RouteNetworkCommand(nameof(NewRouteNodeDigitized), request.CmdId, new List<RouteNetworkEvent> { routeNodeAddedEvent }.ToArray());

            _eventStore.Insert(newRouteNodeDigitizedCommand);

            await Task.FromResult(0);
        }
    }
}
