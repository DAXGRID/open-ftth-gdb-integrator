using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
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
        private readonly ILogger<RouteNodeAddedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IProducer _producer;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IMediator _mediator;

        public RouteNodeLocationChangedHandler(
            ILogger<RouteNodeAddedHandler> logger,
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

            _logger.LogInformation($"Sending {nameof(EventMessages.RouteNodeGeometryModified)} with mrid '{request.RouteNodeAfter.Mrid}' to producer");
            await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                                new EventMessages.RouteNodeGeometryModified
                                                (
                                                    request.CmdId,
                                                    request.RouteNodeAfter.Mrid,
                                                    nameof(RouteNodeLocationChanged),
                                                    request.RouteNodeAfter.GetGeoJsonCoordinate()
                                                ));

            foreach (var routeSegmentToBeUpdated in routeSegmentsToBeUpdated)
            {
                await _geoDatabase.UpdateRouteSegment(routeSegmentToBeUpdated);

                _logger.LogInformation($"Sending {nameof(EventMessages.RouteSegmentGeometryModified)} with mrid '{routeSegmentToBeUpdated.Mrid}' to producer");
                await _producer.Produce(_kafkaSettings.EventRouteNetworkTopicName,
                                        new EventMessages.RouteSegmentGeometryModified
                                        (
                                            request.CmdId,
                                            routeSegmentToBeUpdated.Mrid,
                                            nameof(RouteNodeLocationChanged),
                                            routeSegmentToBeUpdated.GetGeoJsonCoordinate()
                                        ));
            }
        }
    }
}
