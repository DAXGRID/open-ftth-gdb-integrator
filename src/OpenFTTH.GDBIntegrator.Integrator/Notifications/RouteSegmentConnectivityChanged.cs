using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
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
        private readonly IMediator _mediator;

        public RouteSegmentConnectivityChangedHandler(
            ILogger<RouteSegmentConnectivityChangedHandler> logger,
            IOptions<KafkaSetting> kafkaSettings,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory,
            IRouteSegmentFactory routeSegmentFactory,
            IMediator mediator)
        {
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
            _routeSegmentFactory = routeSegmentFactory;
            _mediator = mediator;
        }

        public async Task Handle(RouteSegmentConnectivityChanged request, CancellationToken token)
        {
            var startNode = (await _geoDatabase.GetIntersectingStartRouteNodes(request.After)).FirstOrDefault();
            var endNode = (await _geoDatabase.GetIntersectingEndRouteNodes(request.After)).FirstOrDefault();

            if (startNode is null)
            {
                startNode = _routeNodeFactory.Create(request.After.FindStartPoint());
                await InsertRouteNode(startNode, request.CmdId);
            }
            if (endNode is null)
            {
                endNode = _routeNodeFactory.Create(request.After.FindEndPoint());
                await InsertRouteNode(endNode, request.CmdId);
            }

            var routeSegmentClone = await InsertRouteSegmentClone(request.After, request.CmdId);
            await MarkRouteSegmentForDeletion(request.Before);

            var beforeStartNode = (await _geoDatabase.GetIntersectingStartRouteNodes(request.Before)).FirstOrDefault();
            var beforeEndNode = (await _geoDatabase.GetIntersectingEndRouteNodes(request.Before)).FirstOrDefault();
            var isBeforeStartNodeDeleteable = await IsDeleteable(beforeStartNode);
            var isBeforeEndNodeDeletable = await IsDeleteable(beforeEndNode);

            var isRevertAndDeleteLastEventInCmd = !isBeforeStartNodeDeleteable && !isBeforeEndNodeDeletable;
            await PublishMarkedAsDeletedSegment(
                request.Before,
                request.CmdId,
                routeSegmentClone,
                isRevertAndDeleteLastEventInCmd);

            if (isBeforeStartNodeDeleteable)
                await MarkDeleteRouteNode(beforeStartNode, request.CmdId, !isBeforeEndNodeDeletable);

            if (isBeforeEndNodeDeletable)
                await MarkDeleteRouteNode(beforeEndNode, request.CmdId, true);
        }

        private async Task<bool> IsDeleteable(RouteNode routeNode)
        {
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            return String.IsNullOrEmpty(routeNode.NodeKind)
                && String.IsNullOrEmpty(routeNode.NodeName)
                && intersectingRouteSegments.Count == 0;
        }

        private async Task MarkDeleteRouteNode(RouteNode routeNode, Guid cmdId, bool isLastEventInCmd)
        {
            await _geoDatabase.MarkDeleteRouteNode(routeNode.Mrid);
            await _mediator.Publish(new RouteNodeDeleted
            {
                CmdId = cmdId,
                RouteNode = routeNode,
                IsLastEventInCmd = isLastEventInCmd
            });
        }

        private async Task InsertRouteNode(RouteNode routeNode, Guid cmdId)
        {
            await _geoDatabase.InsertRouteNode(routeNode);
            await _mediator.Publish(new RouteNodeAdded
            {
                CmdId = cmdId,
                CmdType = nameof(RouteSegmentConnectivityChanged),
                RouteNode = routeNode
            });
        }

        private async Task MarkRouteSegmentForDeletion(RouteSegment beforeRouteSegment)
        {
            beforeRouteSegment.MarkAsDeleted = true;
            await _geoDatabase.UpdateRouteSegment(beforeRouteSegment);
        }

        private async Task PublishMarkedAsDeletedSegment
        (
            RouteSegment beforeRouteSegment,
            Guid cmdId,
            RouteSegment replacedBySegment,
            bool isLastEventInCmd
        )
        {
            await _mediator.Publish(
                new RouteSegmentRemoved
                {
                    CmdId = cmdId,
                    RouteSegment = beforeRouteSegment,
                    ReplacedBySegments = new List<Guid> { replacedBySegment.Mrid },
                    CmdType = nameof(RouteSegmentConnectivityChanged),
                    IsLastEventInCmd = isLastEventInCmd
                });
        }


        private async Task<RouteSegment> InsertRouteSegmentClone(RouteSegment routeSegment, Guid cmdId)
        {
            var routeSegmentClone = _routeSegmentFactory.Create(routeSegment.GetLineString());
            await _geoDatabase.InsertRouteSegment(routeSegmentClone);
            await _mediator.Publish(new RouteSegmentAdded
            {
                CmdId = cmdId,
                RouteSegment = routeSegmentClone,
                StartRouteNode = (await _geoDatabase.GetIntersectingStartRouteNodes(routeSegmentClone)).FirstOrDefault(),
                EndRouteNode = (await _geoDatabase.GetIntersectingEndRouteNodes(routeSegmentClone)).FirstOrDefault(),
                CmdType = nameof(RouteSegmentConnectivityChanged),
            });

            return routeSegmentClone;
        }
    }
}
