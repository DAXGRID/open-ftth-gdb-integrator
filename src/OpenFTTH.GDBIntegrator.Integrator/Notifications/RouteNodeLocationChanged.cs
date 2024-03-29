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
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteNodeLocationChanged : INotification
    {
        public RouteNode RouteNodeBefore { get; set; }
        public RouteNode RouteNodeAfter { get; set; }
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
        private readonly IRouteSegmentValidator _routeSegmentValidator;

        public RouteNodeLocationChangedHandler(
            ILogger<RouteNodeLocationChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IEventStore eventStore,
            IGeoDatabase geoDatabase,
            IMediator mediator,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IRouteSegmentValidator routeSegmentValidator)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _eventStore = eventStore;
            _geoDatabase = geoDatabase;
            _mediator = mediator;
            _routeNodeEventFactory = routeNodeEventFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _routeSegmentValidator = routeSegmentValidator;
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
                    throw new InvalidOperationException("Route segments intersects with any route nodes");
                }

                var anyRouteSegmentInvalid = routeSegmentsToBeUpdated.Any(x => !_routeSegmentValidator.LineIsValid(x.GetLineString()).isValid);
                if (anyRouteSegmentInvalid)
                {
                    throw new InvalidOperationException("Route node move results in invalid routesegment geometries.");
                }
            }

            var routeNetworkEvents = new List<RouteNetworkEvent>();

            var routeNodeGeometryModifiedEvent = _routeNodeEventFactory.CreateGeometryModified(request.RouteNodeAfter);
            routeNetworkEvents.Add(routeNodeGeometryModifiedEvent);

            for (var i = 0; i < routeSegmentsToBeUpdated.Count; i++)
            {
                var routeSegmentToBeUpdated = routeSegmentsToBeUpdated[i];
                await _geoDatabase.UpdateRouteSegment(routeSegmentToBeUpdated);

                var routeSegmentGeometryModifiedEvent = _routeSegmentEventFactory.CreateGeometryModified(routeSegmentToBeUpdated, true);
                routeNetworkEvents.Add(routeSegmentGeometryModifiedEvent);
            }

            var cmdId = Guid.NewGuid();
            var routeNodeLocationChangedCommand = new RouteNetworkCommand(nameof(RouteNodeLocationChanged), cmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(routeNodeLocationChangedCommand);
        }
    }
}
