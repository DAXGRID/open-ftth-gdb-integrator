using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.Events.RouteNetwork;
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
        private readonly IProducer _producer;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IMediator _mediator;

        public RouteNodeLocationChangedHandler(
            ILogger<RouteNodeLocationChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IProducer producer,
            IGeoDatabase geoDatabase,
            IMediator mediator)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _producer = producer;
            _geoDatabase = geoDatabase;
        }

        public async Task Handle(RouteNodeLocationChanged request, CancellationToken token)
        {
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

            var routeNodeGeometryModifiedEvent = new RouteNodeGeometryModified
                (
                    nameof(RouteNodeGeometryModified),
                    Guid.NewGuid(),
                    nameof(RouteNodeLocationChanged),
                    request.CmdId,
                    false,
                    request.RouteNodeAfter.WorkTaskMrid,
                    request.RouteNodeAfter.Username,
                    request.RouteNodeAfter?.ApplicationName,
                    request.RouteNodeAfter?.ApplicationInfo,
                    request.RouteNodeAfter.Mrid,
                    request.RouteNodeAfter.GetGeoJsonCoordinate()
                );

            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, routeNodeGeometryModifiedEvent);

            for (var i = 0; i < routeSegmentsToBeUpdated.Count; i++)
            {
                var routeSegmentToBeUpdated = routeSegmentsToBeUpdated[i];
                 await _geoDatabase.UpdateRouteSegment(routeSegmentToBeUpdated);

                 var isLastEventInCmd = i == routeSegmentsToBeUpdated.Count - 1;
                 var routeSegmentGeometryModifiedEvent = new RouteSegmentGeometryModified
                     (
                         nameof(RouteSegmentGeometryModified),
                         Guid.NewGuid(),
                         nameof(RouteNodeLocationChanged),
                         request.CmdId,
                         isLastEventInCmd,
                         routeSegmentToBeUpdated.WorkTaskMrid,
                         routeSegmentToBeUpdated.Username,
                         routeSegmentToBeUpdated?.ApplicationName,
                         routeSegmentToBeUpdated?.ApplicationInfo,
                         routeSegmentToBeUpdated.Mrid,
                         routeSegmentToBeUpdated.GetGeoJsonCoordinate()
                    );

                await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName, routeSegmentGeometryModifiedEvent);
            }
        }
    }
}
