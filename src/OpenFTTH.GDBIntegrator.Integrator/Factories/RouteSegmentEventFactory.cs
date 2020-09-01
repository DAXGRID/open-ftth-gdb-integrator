using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Options;
using MediatR;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentEventFactory : IRouteSegmentEventFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IRouteSegmentValidator _routeSegmentValidator;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;

        public RouteSegmentEventFactory(
            IOptions<ApplicationSetting> applicationSettings,
            IRouteSegmentValidator routeSegmentValidator,
            IGeoDatabase geoDatabase,
            IRouteNodeFactory routeNodeFactory)
        {
            _applicationSettings = applicationSettings.Value;
            _routeSegmentValidator = routeSegmentValidator;
            _geoDatabase = geoDatabase;
            _routeNodeFactory = routeNodeFactory;
        }

        public async Task<IEnumerable<INotification>> CreateUpdatedEvent(RouteSegment before, RouteSegment after)
        {
            var routeSegmentShadowTableBeforeUpdate = await _geoDatabase.GetRouteSegmentShadowTable(after.Mrid);

            if (routeSegmentShadowTableBeforeUpdate is null)
                return new List<INotification> { new DoNothing($"{nameof(RouteSegment)} is already deleted, therefore do nothing") };

            if (AlreadyUpdated(after, routeSegmentShadowTableBeforeUpdate))
                return new List<INotification> { new DoNothing($"{nameof(RouteSegment)} is already updated, therefore do nothing.") };

            if (!_routeSegmentValidator.LineIsValid(after.GetLineString()))
                return new List<INotification> { new RollbackInvalidRouteSegment(before) };

            await _geoDatabase.UpdateRouteSegmentShadowTable(after);

            var cmdId = Guid.NewGuid();

            if (after.MarkAsDeleted)
                return new List<INotification> { CreateRouteSegmentDeleted(after, cmdId, true) };

            var intersectingStartSegments = await _geoDatabase.GetIntersectingStartRouteSegments(after);
            var intersectingEndSegments = await _geoDatabase.GetIntersectingEndRouteSegments(after);
            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(after);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(after);
            var allIntersectingRouteNodesNoEdges = await _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(after);

            if (await IsGeometryChanged(intersectingStartNodes.FirstOrDefault(), intersectingEndNodes.FirstOrDefault(), before))
            {
                var events = new List<INotification>();
                events.Add(new RouteSegmentLocationChanged { CmdId = cmdId, RouteSegment = after, IsLastEventInCmd = true });

                if (allIntersectingRouteNodesNoEdges.Count > 0)
                {
                    foreach (var intersectingRouteNode in allIntersectingRouteNodesNoEdges)
                    {
                        var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(null, cmdId, intersectingRouteNode);
                        events.Add(routeSegmentSplitted);
                    }
                }

                return events;
            }

            var notifications = new List<INotification>();
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingStartSegments.Count, intersectingStartNodes.Count, cmdId, after.FindStartPoint(), after));
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingEndSegments.Count, intersectingEndNodes.Count, cmdId, after.FindEndPoint(), after));

            notifications.Add(new RouteSegmentConnectivityChanged(before, after, cmdId));

            return notifications;
        }

        public async Task<bool> IsGeometryChanged(RouteNode startNode, RouteNode endNode, RouteSegment before)
        {
            if (startNode is null || endNode is null)
                return false;

            var previousStartNode = (await _geoDatabase.GetIntersectingStartRouteNodes(before.Coord)).First();
            var previousEndNode = (await _geoDatabase.GetIntersectingEndRouteNodes(before.Coord)).First();

            var routeSegmentHasSameStartRouteNode = startNode.Mrid == previousStartNode.Mrid;
            var routeSegmentHasSameEndRouteNode = endNode.Mrid == previousEndNode.Mrid;
            if (routeSegmentHasSameStartRouteNode && routeSegmentHasSameEndRouteNode)
                return true;

            return false;
        }

        public async Task<IEnumerable<INotification>> CreateDigitizedEvent(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"Parameter {nameof(routeSegment)} must not be null");

            if (IsCreatedByApplication(routeSegment))
                return new List<INotification>();

            // Update integrator "shadow table" with the used digitized segment
            await _geoDatabase.InsertRouteSegmentShadowTable(routeSegment);

            var cmdId = Guid.NewGuid();

            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new List<INotification> { new InvalidRouteSegmentOperation { RouteSegment = routeSegment, CmdId = cmdId } };

            var intersectingStartNodesTask = _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodesTask = _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);
            var intersectingStartSegmentsTask = _geoDatabase.GetIntersectingStartRouteSegments(routeSegment);
            var intersectingEndSegmentsTask = _geoDatabase.GetIntersectingEndRouteSegments(routeSegment);
            var allIntersectingRouteNodesNoEdgesTask = _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment);

            var intersectingStartNodes = await intersectingStartNodesTask;
            var intersectingEndNodes = await intersectingEndNodesTask;
            var intersectingStartSegments = await intersectingStartSegmentsTask;
            var intersectingEndSegments = await intersectingEndSegmentsTask;
            var allIntersectingRouteNodesNoEdges = await allIntersectingRouteNodesNoEdgesTask;

            var notifications = new List<INotification>();

            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingStartSegments.Count, intersectingStartNodes.Count, cmdId, routeSegment.FindStartPoint(), routeSegment));
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingEndSegments.Count, intersectingEndNodes.Count, cmdId, routeSegment.FindEndPoint(), routeSegment));

            notifications.Add(CreateNewRouteSegmentDigitized(routeSegment, cmdId));

            if (allIntersectingRouteNodesNoEdges.Count > 0)
            {
                foreach (var intersectingRouteNode in allIntersectingRouteNodesNoEdges)
                {
                    var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(null, cmdId, intersectingRouteNode);
                    notifications.Add(routeSegmentSplitted);
                }
            }

            return notifications;
        }

        private List<INotification> HandleExistingRouteSegmentSplitted(int intersectingSegmentsCount, int intersectingNodesCount, Guid cmdId, Point point, RouteSegment routeSegment)
        {
            var notifications = new List<INotification>();

            if (intersectingSegmentsCount == 1 && intersectingNodesCount == 0)
            {
                var node = _routeNodeFactory.Create(point);
                notifications.Add(new NewRouteNodeDigitized
                {
                    CmdId = cmdId,
                    RouteNode = node,
                    IsLastEventInCmd = false,
                    CmdType = nameof(ExistingRouteSegmentSplitted)
                });

                var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(routeSegment, cmdId, node);
                notifications.Add(routeSegmentSplitted);
            }

            return notifications;
        }

        private bool AlreadyUpdated(RouteSegment routeSegment, RouteSegment shadowTableRouteSegment)
        {
            return routeSegment.MarkAsDeleted == shadowTableRouteSegment.MarkAsDeleted && routeSegment.GetGeoJsonCoordinate() == shadowTableRouteSegment.GetGeoJsonCoordinate();
        }

        private bool IsCreatedByApplication(RouteSegment routeSegment)
        {
            return routeSegment.ApplicationName == _applicationSettings.ApplicationName;
        }

        private RouteSegmentDeleted CreateRouteSegmentDeleted(RouteSegment routeSegment, Guid cmdId, bool isLastEventInCommand)
        {
            return new RouteSegmentDeleted
            {
                RouteSegment = routeSegment,
                CmdId = cmdId,
                IsLastEventInCmd = isLastEventInCommand
            };
        }

        private ExistingRouteSegmentSplitted CreateExistingRouteSegmentSplitted(RouteSegment routeSegment, Guid cmdId, RouteNode routeNode)
        {
            return new ExistingRouteSegmentSplitted
            {
                RouteNode = routeNode,
                CmdId = cmdId,
                RouteSegmentDigitizedByUser = routeSegment,
            };
        }

        private NewRouteSegmentDigitized CreateNewRouteSegmentDigitized(RouteSegment routeSegment, Guid cmdId)
        {
            return new NewRouteSegmentDigitized
            {
                RouteSegment = routeSegment,
                EventId = cmdId
            };
        }
    }
}
