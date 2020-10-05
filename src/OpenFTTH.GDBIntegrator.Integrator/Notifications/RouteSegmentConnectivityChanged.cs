using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.Events;
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
    public class RouteSegmentConnectivityChanged : INotification
    {
        public RouteSegment Before { get; }
        public RouteSegment After { get; }

        public RouteSegmentConnectivityChanged(RouteSegment before, RouteSegment after)
        {
            Before = before;
            After = after;
        }
    }

    public class RouteSegmentConnectivityChangedHandler : INotificationHandler<RouteSegmentConnectivityChanged>
    {
        private readonly ILogger<RouteSegmentConnectivityChangedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly ApplicationSetting _applicationSettings;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;
        private readonly IRouteSegmentFactory _routeSegmentFactory;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentConnectivityChangedHandler(
            ILogger<RouteSegmentConnectivityChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IOptions<ApplicationSetting> applicationSettings,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory,
            IRouteSegmentFactory routeSegmentFactory,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _applicationSettings = applicationSettings.Value;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
            _routeSegmentFactory = routeSegmentFactory;
            _routeNodeEventFactory = routeNodeEventFactory;
            _routeSegmentEventFactory = routeSegmentEventFactory;
            _eventStore = eventStore;
        }

        public async Task Handle(RouteSegmentConnectivityChanged request, CancellationToken token)
        {
            _logger.LogInformation($"Starting {nameof(RouteSegmentConnectivityChangedHandler)}");

            var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(request.After)).FirstOrDefault();
            var endNode = (await _geoDatabase.GetIntersectingEndRouteNodes(request.After)).FirstOrDefault();

            if ((!(startNode is null) && !(endNode is null)) && startNode.Mrid == endNode.Mrid)
            {
                _logger.LogWarning($"Reverting RouteSegment with mrid '{request.After.Mrid}', because of both ends intersecting with the same RouteNode with mrid '{startNode.Mrid}'");
                await _geoDatabase.UpdateRouteSegment(request.Before);
                return;
            }

            var routeNetworkEvents = new List<RouteNetworkEvent>();

            if (startNode is null)
            {
                startNode = _routeNodeFactory.Create(request.After.FindStartPoint());
                var insertRouteNodeEvent = await InsertRouteNode(startNode);
                routeNetworkEvents.Add(insertRouteNodeEvent);
            }
            else if (_applicationSettings.EnableSegmentEndsAutoSnappingToRouteNode)
            {
                var lineString = request.After.GetLineString();
                lineString.Coordinates[0] = new Coordinate(startNode.GetPoint().Coordinate);
                request.After.Coord = lineString.AsBinary();
                await _geoDatabase.UpdateRouteSegment(request.After);
            }

            if (endNode is null)
            {
                endNode = _routeNodeFactory.Create(request.After.FindEndPoint());
                var insertRouteNodeEvent = await InsertRouteNode(endNode);
                routeNetworkEvents.Add(insertRouteNodeEvent);
            }
            else if (_applicationSettings.EnableSegmentEndsAutoSnappingToRouteNode)
            {
                var lineString = request.After.GetLineString();
                lineString.Coordinates[lineString.Coordinates.Count() - 1] = new Coordinate(endNode.GetPoint().Coordinate);
                request.After.Coord = lineString.AsBinary();
                await _geoDatabase.UpdateRouteSegment(request.After);
            }

            var (routeSegmentClone, routeSegmentAddedEvent) = await InsertRouteSegmentClone(request.After);
            routeNetworkEvents.Add(routeSegmentAddedEvent);

            var routeSegmentMarkedForDeletionEvent = await MarkRouteSegmentForDeletion(request.Before);
            routeNetworkEvents.Add(routeSegmentMarkedForDeletionEvent);

            var beforeStartNode = (await _geoDatabase.GetIntersectingStartRouteNodes(request.Before)).FirstOrDefault();
            var beforeEndNode = (await _geoDatabase.GetIntersectingEndRouteNodes(request.Before)).FirstOrDefault();
            var isBeforeStartNodeDeleteable = await IsDeleteable(beforeStartNode);
            var isBeforeEndNodeDeletable = await IsDeleteable(beforeEndNode);

            if (isBeforeStartNodeDeleteable)
            {
                var routeNodeMarkedForDeletionEvent = await MarkDeleteRouteNode(beforeStartNode);
                routeNetworkEvents.Add(routeNodeMarkedForDeletionEvent);
            }

            if (isBeforeEndNodeDeletable)
            {
                var routeNodeMarkedForDeletionEvent = await MarkDeleteRouteNode(beforeEndNode);
                routeNetworkEvents.Add(routeNodeMarkedForDeletionEvent);
            }

            var cmdId = Guid.NewGuid();
            var routeSegmentConnectivityChangedEvent = new RouteNetworkCommand(nameof(RouteSegmentConnectivityChanged), cmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(routeSegmentConnectivityChangedEvent);
        }

        private async Task<bool> IsDeleteable(RouteNode routeNode)
        {
            var intersectingStartRouteSegments = await _geoDatabase.GetIntersectingStartRouteSegments(routeNode);
            var intersectingEndRouteSegments = await _geoDatabase.GetIntersectingEndRouteSegments(routeNode);

            var intersectingRouteSegmentsCount = intersectingStartRouteSegments.Count + intersectingEndRouteSegments.Count;

            return routeNode.RouteNodeInfo?.Kind == null
                && String.IsNullOrEmpty(routeNode.NamingInfo?.Name)
                && intersectingRouteSegmentsCount == 0;
        }

        private async Task<RouteNodeMarkedForDeletion> MarkDeleteRouteNode(RouteNode routeNode)
        {
            await _geoDatabase.MarkDeleteRouteNode(routeNode.Mrid);
            routeNode.ApplicationName = _applicationSettings.ApplicationName;
            return _routeNodeEventFactory.CreateMarkedForDeletion(routeNode);
        }

        private async Task<RouteNodeAdded> InsertRouteNode(RouteNode routeNode)
        {
            await _geoDatabase.InsertRouteNode(routeNode);
            routeNode.ApplicationName = _applicationSettings.ApplicationName;
            return _routeNodeEventFactory.CreateAdded(routeNode);
        }

        private async Task<RouteSegmentMarkedForDeletion> MarkRouteSegmentForDeletion(RouteSegment beforeRouteSegment)
        {
            beforeRouteSegment.MarkAsDeleted = true;
            beforeRouteSegment.ApplicationName = _applicationSettings.ApplicationName;
            await _geoDatabase.UpdateRouteSegment(beforeRouteSegment);

            return _routeSegmentEventFactory.CreateMarkedForDeletion(beforeRouteSegment);
        }

        private async Task<(RouteSegment, RouteSegmentAdded)> InsertRouteSegmentClone(RouteSegment routeSegment)
        {
            var routeSegmentClone = CreateRouteSegmentClone(routeSegment);

            await _geoDatabase.InsertRouteSegment(routeSegmentClone);

            var startRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegmentClone)).FirstOrDefault();
            var endRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegmentClone)).FirstOrDefault();

            var routeSegmentAddedEvent = _routeSegmentEventFactory.CreateAdded(routeSegmentClone, startRouteNode, endRouteNode);

            return (routeSegmentClone, routeSegmentAddedEvent);
        }

        private RouteSegment CreateRouteSegmentClone(RouteSegment routeSegment)
        {
            var routeSegmentClone = _routeSegmentFactory.Create(routeSegment.GetLineString());
            routeSegmentClone.WorkTaskMrid = routeSegment.WorkTaskMrid;
            routeSegmentClone.ApplicationInfo = routeSegment.ApplicationInfo;
            routeSegmentClone.MappingInfo = routeSegment.MappingInfo;
            routeSegmentClone.LifeCycleInfo = routeSegment.LifeCycleInfo;
            routeSegmentClone.NamingInfo = routeSegment.NamingInfo;
            routeSegmentClone.RouteSegmentInfo = routeSegment.RouteSegmentInfo;
            routeSegmentClone.SafetyInfo = routeSegment.SafetyInfo;
            routeSegmentClone.Username = routeSegment.Username;

            return routeSegmentClone;
        }
    }
}
