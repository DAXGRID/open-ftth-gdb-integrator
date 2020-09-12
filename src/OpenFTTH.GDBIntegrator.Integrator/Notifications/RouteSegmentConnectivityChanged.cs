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

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class RouteSegmentConnectivityChanged : INotification
    {
        public RouteSegment Before { get; }
        public RouteSegment After { get; }
        public Guid CmdId { get; }

        public RouteSegmentConnectivityChanged(RouteSegment before, RouteSegment after, Guid cmdId)
        {
            Before = before;
            After = after;
            CmdId = cmdId;
        }
    }

    public class RouteSegmentConnectivityChangedHandler : INotificationHandler<RouteSegmentConnectivityChanged>
    {
        private readonly ILogger<RouteSegmentConnectivityChangedHandler> _logger;
        private readonly KafkaSetting _kafkaSettings;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;
        private readonly IRouteSegmentFactory _routeSegmentFactory;
        private readonly IRouteNodeEventFactory _routeNodeEventFactory;
        private readonly IRouteSegmentEventFactory _routeSegmentEventFactory;
        private readonly IEventStore _eventStore;

        public RouteSegmentConnectivityChangedHandler(
            ILogger<RouteSegmentConnectivityChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory,
            IRouteSegmentFactory routeSegmentFactory,
            IRouteNodeEventFactory routeNodeEventFactory,
            IRouteSegmentEventFactory routeSegmentEventFactory,
            IEventStore eventStore)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
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

            var routeNetworkEvents = new List<RouteNetworkEvent>();

            if (startNode is null)
            {
                startNode = _routeNodeFactory.Create(request.After.FindStartPoint());
                var insertRouteNodeEvent = await InsertRouteNode(startNode);
                routeNetworkEvents.Add(insertRouteNodeEvent);
            }
            if (endNode is null)
            {
                endNode = _routeNodeFactory.Create(request.After.FindEndPoint());
                var insertRouteNodeEvent = await InsertRouteNode(endNode);
                routeNetworkEvents.Add(insertRouteNodeEvent);
            }

            var (routeSegmentClone, routeSegmentAddedEvent) = await InsertRouteSegmentClone(request.After);
            routeNetworkEvents.Add(routeSegmentAddedEvent);

            await MarkRouteSegmentForDeletion(request.Before);

            var beforeStartNode = (await _geoDatabase.GetIntersectingStartRouteNodes(request.Before)).FirstOrDefault();
            var beforeEndNode = (await _geoDatabase.GetIntersectingEndRouteNodes(request.Before)).FirstOrDefault();
            var isBeforeStartNodeDeleteable = await IsDeleteable(beforeStartNode);
            var isBeforeEndNodeDeletable = await IsDeleteable(beforeEndNode);

            var routeSegmentMarkedForDeletionEvent = _routeSegmentEventFactory.CreateMarkedForDeletion(routeSegmentClone);
            routeNetworkEvents.Add(routeSegmentMarkedForDeletionEvent);

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

            var routeSegmentConnectivityChangedEvent = new RouteNetworkCommand(nameof(RouteSegmentConnectivityChanged), request.CmdId, routeNetworkEvents.ToArray());
            _eventStore.Insert(routeSegmentConnectivityChangedEvent);
        }

        private async Task<bool> IsDeleteable(RouteNode routeNode)
        {
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            return routeNode.RouteNodeInfo?.Kind == null
                && String.IsNullOrEmpty(routeNode.NamingInfo?.Name)
                && intersectingRouteSegments.Count == 0;
        }

        private async Task<RouteNodeMarkedForDeletion> MarkDeleteRouteNode(RouteNode routeNode)
        {
            await _geoDatabase.MarkDeleteRouteNode(routeNode.Mrid);
            return _routeNodeEventFactory.CreateMarkedForDeletion(routeNode);
        }

        private async Task<RouteNodeAdded> InsertRouteNode(RouteNode routeNode)
        {
            await _geoDatabase.InsertRouteNode(routeNode);
            return _routeNodeEventFactory.CreateAdded(routeNode);
        }

        private async Task MarkRouteSegmentForDeletion(RouteSegment beforeRouteSegment)
        {
            beforeRouteSegment.MarkAsDeleted = true;
            await _geoDatabase.UpdateRouteSegment(beforeRouteSegment);
        }

        private async Task<(RouteSegment, RouteSegmentAdded)> InsertRouteSegmentClone(RouteSegment routeSegment)
        {
            var routeSegmentClone = _routeSegmentFactory.Create(routeSegment.GetLineString());
            await _geoDatabase.InsertRouteSegment(routeSegmentClone);

            var startRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegmentClone)).FirstOrDefault();
            var endRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegmentClone)).FirstOrDefault();

            var routeSegmentAddedEvent = _routeSegmentEventFactory.CreateAdded(routeSegmentClone, startRouteNode, endRouteNode);

            return (routeSegmentClone, routeSegmentAddedEvent);
        }
    }
}
