using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class ExistingRouteSegmentSplittedByUser : INotification
    {
        public RouteNode RouteNode { get; set; }
        public Guid EventId { get; set; }
        public RouteSegment RouteSegmentDigitizedByUser { get; set; }
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

            var intersectingRouteSegment = await GetIntersectingRouteSegment(request.RouteSegmentDigitizedByUser, request.RouteNode);

            var routeSegmentsWkt = await _geoDatabase.GetRouteSegmentsSplittedByRouteNode(request.RouteNode, intersectingRouteSegment);
            var routeSegments = _routeSegmentFactory.Create(routeSegmentsWkt);

            await InsertReplacementRouteSegments(routeSegments, request.EventId);

            await MarkExistingRouteSegmentForDeletion(intersectingRouteSegment, request.EventId, routeSegments);
        }

        private async Task<RouteSegment> GetIntersectingRouteSegment(RouteSegment routeSegmentDigitizedByUser, RouteNode routeNode)
        {
            RouteSegment intersectingRouteSegment = null;
            if (routeSegmentDigitizedByUser is null)
            {
                intersectingRouteSegment = await HandleIntersectionSplit(routeNode);
            }
            // This is required in case that this event was triggered by RouteSegmentDigtizedByUser
            else
            {
                intersectingRouteSegment = (await _geoDatabase.GetIntersectingRouteSegments(routeNode, routeSegmentDigitizedByUser)).First();
            }

            return intersectingRouteSegment;
        }

        private async Task<RouteSegment> HandleIntersectionSplit(RouteNode routeNode)
        {
            RouteSegment intersectingRouteSegment = null;
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            foreach (var individualIntersectingRouteSegment in intersectingRouteSegments)
            {
                intersectingRouteSegment = individualIntersectingRouteSegment;
                var intersectingRouteNodesCount = (await _geoDatabase.GetAllIntersectingRouteNodes(individualIntersectingRouteSegment)).Count;

                if (intersectingRouteNodesCount >= 3)
                    break;
            }

            return intersectingRouteSegment;
        }

        private async Task InsertReplacementRouteSegments(List<RouteSegment> routeSegments, Guid eventId)
        {
            foreach (var routeSegment in routeSegments)
            {
                _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Inserting routesegment: {routeSegment.Mrid}");
                await _geoDatabase.InsertRouteSegment(routeSegment);
                await _mediator.Publish(new RouteSegmentAdded
                {
                    EventId = eventId,
                    RouteSegment = routeSegment,
                    StartRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment)).FirstOrDefault(),
                    EndRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment)).FirstOrDefault(),
                    CmdType = nameof(ExistingRouteSegmentSplittedByUser)
                });
            }
        }

        private async Task MarkExistingRouteSegmentForDeletion(RouteSegment intersectingRouteSegment, Guid eventId, List<RouteSegment> routeSegments)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Deleting routesegment: {intersectingRouteSegment.Mrid}");
            await _geoDatabase.MarkDeleteRouteSegment(intersectingRouteSegment.Mrid);
            await _mediator.Publish(
                new RouteSegmentRemoved
                {
                    EventId = eventId,
                    RouteSegment = intersectingRouteSegment,
                    ReplacedBySegments = routeSegments.Select(x => x.Mrid),
                    CmdType = nameof(ExistingRouteSegmentSplittedByUser)
                });
        }
    }
}
