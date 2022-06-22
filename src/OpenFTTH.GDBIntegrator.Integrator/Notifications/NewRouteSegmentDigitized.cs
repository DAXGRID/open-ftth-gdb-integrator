using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using OpenFTTH.Events;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class NewRouteSegmentDigitized : INotification
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewRouteSegmentDigitizedHandler : INotificationHandler<NewRouteSegmentDigitized>
    {
        private readonly ILogger<NewRouteSegmentDigitizedHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly IEventStore _eventStore;
        private readonly ApplicationSetting _applicationSettings;

        public NewRouteSegmentDigitizedHandler(
            ILogger<NewRouteSegmentDigitizedHandler> logger,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IEventStore eventStore,
            IOptions<ApplicationSetting> applicationSettings)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _eventStore = eventStore;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(NewRouteSegmentDigitized request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(NewRouteSegmentDigitizedHandler)}");

            if (request.RouteSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be null.");

            var routeSegment = request.RouteSegment;
            var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault();
            var endNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault();

            if ((!(startNode is null) && !(endNode is null)) && startNode.Mrid == endNode.Mrid)
            {
                _logger.LogWarning($"Deleting RouteSegment with mrid '{routeSegment.Mrid}', because of both ends intersecting with the same RouteNode with mrid '{startNode.Mrid}'");
                await _geoDatabase.DeleteRouteSegment(routeSegment.Mrid);
                return;
            }

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
            else if (_applicationSettings.EnableSegmentEndsAutoSnappingToRouteNode)
            {
                var lineString = routeSegment.GetLineString();
                lineString.Coordinates[0] = new Coordinate(startNode.GetPoint().Coordinate);
                routeSegment.Coord = lineString.AsBinary();
                await _geoDatabase.UpdateRouteSegment(routeSegment);
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
            else if (_applicationSettings.EnableSegmentEndsAutoSnappingToRouteNode)
            {
                var lineString = routeSegment.GetLineString();
                lineString.Coordinates[lineString.Coordinates.Count() - 1] = new Coordinate(endNode.GetPoint().Coordinate);
                routeSegment.Coord = lineString.AsBinary();
                await _geoDatabase.UpdateRouteSegment(routeSegment);
            }

            var routeSegmentAddedEvent = _routeSegmentEventFactory.CreateAdded(routeSegment, startNode, endNode);
            routeNetworkEvents.Add(routeSegmentAddedEvent);

            var cmdId = Guid.NewGuid();
            var newRouteSegmentDigitizedCommand = new RouteNetworkCommand(nameof(NewRouteSegmentDigitized), cmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(newRouteSegmentDigitizedCommand);
        }
    }
}
