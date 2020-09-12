using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.Events;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeLocationChanged : INotification
    {
        public RouteNode RouteNodeBefore { get; set; }
        public RouteNode RouteNodeAfter { get; set; }
        public Guid CmdId { get; set; }
    }

    public class RouteNodeLocationChangedHandler : INotificationHandler<RouteNodeLocationChanged>
    {
        private readonly ILogger<RouteNodeLocationChangedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IMediator _mediator;
        private readonly IEventStore _eventStore;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;

        public RouteNodeLocationChangedHandler(
            ILogger<RouteNodeLocationChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IEventStore eventStore,
            IGeoDatabase geoDatabase,
            IMediator mediator,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
            _geoDatabase = geoDatabase;
            _mediator = mediator;
            _routeNodeEventFactory = routeNodeEventFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
        }

        public async Task Handle(RouteNodeLocationChanged request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(RouteNodeLocationChangedHandler)}");

            var intersectingSegmentsBeforeChange = (await _geoDatabase.GetIntersectingRouteSegments(request.RouteNodeBefore.Coord)).ToList();
            var routeSegmentsToBeUpdated = new List<RouteSegment>();
            if (intersectingSegmentsBeforeChange.Count > 0)
            {
                var anySegmentIntersectRouteNode = false;
                foreach (var intersectingSegment in intersectingSegmentsBeforeChange)
                {
                    var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(intersectingSegment)).FirstOrDefault();

                    if (startNode is null)
                    {
                        var lineString = intersectingSegment.GetLineString();
                        lineString.Coordinates[0] = new Coordinate(request.RouteNodeAfter.GetPoint().Coordinate);
                        intersectingSegment.Coord = lineString.AsBinary();
                    }
                    else
                    {
                        var lineString = intersectingSegment.GetLineString();
                        lineString.Coordinates[lineString.Coordinates.Count() - 1] = new Coordinate(request.RouteNodeAfter.GetPoint().Coordinate);
                        intersectingSegment.Coord = lineString.AsBinary();
                    }

                    anySegmentIntersectRouteNode = (await _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(intersectingSegment.Coord, intersectingSegment.Mrid)).Any();
                    if (anySegmentIntersectRouteNode)
                        break;

                    routeSegmentsToBeUpdated.Add(intersectingSegment);
                }

                // Rollback in case of segment intersecting with any route nodes
                if (anySegmentIntersectRouteNode)
                {
                    await _mediator.Publish(new RollbackInvalidRouteNode(request.RouteNodeBefore));
                    return;
                }
            }

            var routeNetworkEvents = new List<RouteNetworkEvent>();

            var routeNodeGeometryModifiedEvent = _routeNodeEventFactory.CreateGeometryModified(request.RouteNodeAfter);
            routeNetworkEvents.Add(routeNodeGeometryModifiedEvent);

            for (var i = 0; i < routeSegmentsToBeUpdated.Count; i++)
            {
                var routeSegmentToBeUpdated = routeSegmentsToBeUpdated[i];
                 await _geoDatabase.UpdateRouteSegment(routeSegmentToBeUpdated);

                 var routeSegmentGeometryModifiedEvent = _routeSegmentEventFactory.CreateGeometryModified(routeSegmentToBeUpdated);
                 routeNetworkEvents.Add(routeSegmentGeometryModifiedEvent);
            }

            var routeNodeLocationChangedCommand = new RouteNetworkCommand(nameof(RouteNodeLocationChanged), request.CmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(routeNodeLocationChangedCommand);
        }
    }
}
