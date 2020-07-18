using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class ExistingRouteSegmentSplittedByUser : INotification
    {
        public RouteNode RouteNode { get; set; }
        public RouteSegment IntersectingRouteSegment { get; set; }
        public Guid EventId { get; set; }
    }

    public class ExistingRouteSegmentSplittedByUserHandler : INotificationHandler<ExistingRouteSegmentSplittedByUser>
    {
        private readonly ILogger<ExistingRouteSegmentSplittedByUserHandler> _logger;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteSegmentFactory _routeSegmentFactory;
        private readonly IMediator _mediator;

        public ExistingRouteSegmentSplittedByUserHandler(
            ILogger<ExistingRouteSegmentSplittedByUserHandler> logger,
            IGeoDatabase geoDatabase,
            IRouteSegmentFactory routeSegmentFactory,
            IMediator mediator)
        {
            _logger = logger;
            _geoDatabase = geoDatabase;
            _routeSegmentFactory = routeSegmentFactory;
            _mediator = mediator;
        }

        public async Task Handle(ExistingRouteSegmentSplittedByUser request, CancellationToken token)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Starting Existing route segment splitted by route node");

            var routeSegmentsWkt = await _geoDatabase.GetRouteSegmentsSplittedByRouteNode(request.RouteNode, request.IntersectingRouteSegment);
            var routeSegments = _routeSegmentFactory.Create(routeSegmentsWkt);

            foreach (var routeSegment in routeSegments)
            {
                _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Inserting routesegment: {routeSegment.Mrid}");
                await _geoDatabase.InsertRouteSegment(routeSegment);
                // await _mediator.Publish(new RouteSegmentAdded
                //     {
                //         EventId = request.EventId,
                //         RouteSegment = routeSegment,
                //         StartRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault(),
                //         EndRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault()
                //     });
            }

            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Deleting routesegment: {request.IntersectingRouteSegment.Mrid}");
            await _geoDatabase.DeleteRouteSegment(request.IntersectingRouteSegment.Mrid);
            await _mediator.Publish(
                new RouteSegmentRemoved
                { EventId = request.EventId,
                  RouteSegment = request.IntersectingRouteSegment,
                  ReplacedBySegments = routeSegments.Select(x => x.Mrid)
                });
        }
    }
}
