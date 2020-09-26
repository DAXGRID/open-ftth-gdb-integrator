using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteSegmentDigitized : INotification
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewRouteSegmentDigitizedHandler : INotificationHandler<NewRouteSegmentDigitized>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewRouteSegmentDigitizedHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly IEventStore _eventStore;

        public NewRouteSegmentDigitizedHandler(
            IMediator mediator,
            ILogger<NewRouteSegmentDigitizedHandler> logger,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IEventStore eventStore)
        {
            _mediator = mediator;
            _logger = logger;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(NewRouteSegmentDigitized request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(NewRouteSegmentDigitizedHandler)}");

            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            var routeSegment = request.RouteSegment;
            var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault();
            var endNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault();

            var routeNetworkEvents = new List<RouteNetworkEvent>();

            if (startNode is null)
            {
                var startPoint = routeSegment.FindStartPoint();
                startNode = _routeNodeFactory.Create(startPoint);
                startNode.Username = routeSegment.Username;
                startNode.WorkTaskMrid = routeSegment.WorkTaskMrid;

                await _geoDatabase.InsertRouteNode(startNode);

                var startRouteNodeAddedEvent = _routeNodeEventFactory.CreateAdded(startNode);
                routeNetworkEvents.Add(startRouteNodeAddedEvent);
            }

            if (endNode is null)
            {
                var endPoint = routeSegment.FindEndPoint();
                endNode = _routeNodeFactory.Create(endPoint);
                endNode.Username = routeSegment.Username;
                endNode.WorkTaskMrid = routeSegment.WorkTaskMrid;

                await _geoDatabase.InsertRouteNode(endNode);

                var endRouteNodeAddedEvent = _routeNodeEventFactory.CreateAdded(endNode);
                routeNetworkEvents.Add(endRouteNodeAddedEvent);
            }

            var routeSegmentAddedEvent = _routeSegmentEventFactory.CreateAdded(routeSegment, startNode, endNode);
            routeNetworkEvents.Add(routeSegmentAddedEvent);

            var cmdId = Guid.NewGuid();
            var newRouteSegmentDigitizedCommand = new RouteNetworkCommand(nameof(NewRouteSegmentDigitized), cmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(newRouteSegmentDigitizedCommand);
        }
    }
}
