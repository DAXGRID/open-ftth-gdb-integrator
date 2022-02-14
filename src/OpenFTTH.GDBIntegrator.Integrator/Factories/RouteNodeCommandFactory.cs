using MediatR;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeCommandFactory : IRouteNodeCommandFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeCommandFactory(
            IOptions<ApplicationSetting> applicationSettings,
            IGeoDatabase geoDatabase)
        {
            _applicationSettings = applicationSettings.Value;
            _geoDatabase = geoDatabase;
        }

        public async Task<List<INotification>> CreateUpdatedEvent(RouteNode before, RouteNode after)
        {
            if (before is null || after is null)
                throw new ArgumentNullException($"Parameter {nameof(before)} or {nameof(after)} cannot both be null doing an update.");

            var shadowTableNode = await _geoDatabase.GetRouteNodeShadowTable(after.Mrid, false);

            if (shadowTableNode is null)
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} is already deleted, so do nothing.") };

            if (AlreadyUpdated(after, shadowTableNode))
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} with id: '{after.Mrid}' was already updated therefore do nothing.") };

            if (IsModifiedDistanceLessThanTolerance(shadowTableNode, after))
                return new List<INotification> { new RollbackInvalidRouteNode(shadowTableNode, "Modified distance less than tolerance.") };

            if (!(await IsValidNodeUpdate(shadowTableNode, after)))
                return new List<INotification> { new RollbackInvalidRouteNode(shadowTableNode, "Is not a valid route node update.") };

            await _geoDatabase.UpdateRouteNodeShadowTable(after);

            // We roll back in-case the update-command intersects with new route-segments
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);
            if (intersectingRouteSegments.Count > 0)
            {
                var previousIntersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(shadowTableNode.Coord);
                var newIntersectingRouteSegments = intersectingRouteSegments
                    .Where(x => !previousIntersectingRouteSegments.Any(y => y.Mrid == x.Mrid)).ToList();
                if (newIntersectingRouteSegments.Count > 0)
                    return new List<INotification> { new RollbackInvalidRouteNode(shadowTableNode, "Update to route node is invalid because it is insecting with route-segments.") };
            }

            if (after.MarkAsDeleted)
                return new List<INotification> { new RouteNodeDeleted { RouteNode = after } };

            return new List<INotification> { new RouteNodeLocationChanged { RouteNodeAfter = after, RouteNodeBefore = shadowTableNode } };
        }

        public async Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            if (IsCreatedByApplication(routeNode))
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} with id: '{routeNode.Mrid}' was created by {routeNode.ApplicationName} therefore do nothing.") };

            await _geoDatabase.InsertRouteNodeShadowTable(routeNode);

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(routeNode);

            if (intersectingRouteNodes.Count > 0)
            {
                return new List<INotification> { new InvalidRouteNodeOperation { RouteNode = routeNode, Message = "RouteNode intersects with another RouteNode" } };
            }

            if (intersectingRouteSegments.Count == 0)
                return new List<INotification> { new NewRouteNodeDigitized { RouteNode = routeNode } };

            if (intersectingRouteSegments.Count == 1)
            {
                var notifications = new List<INotification>();
                notifications.Add(new ExistingRouteSegmentSplitted
                {
                    RouteNode = routeNode,
                    InsertNode = false,
                    CreateNodeAddedEvent = true
                });

                return notifications;
            }

            return new List<INotification> { new InvalidRouteNodeOperation { RouteNode = routeNode, Message = "Route node did not fit any condition in command factory." } };
        }

        private async Task<bool> IsValidNodeUpdate(RouteNode shadowTableNode, RouteNode after)
        {
            var startRouteSegment = await _geoDatabase.GetIntersectingStartRouteSegments(shadowTableNode);
            var endRouteSegment = await _geoDatabase.GetIntersectingEndRouteSegments(shadowTableNode);
            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(after);

            if (((startRouteSegment.Count + endRouteSegment.Count) > 0 && after.MarkAsDeleted) || intersectingRouteNodes.Count > 0)
                return false;

            return true;
        }

        private bool AlreadyUpdated(RouteNode routeNode, RouteNode routeNodeShadowTable)
            => routeNode.MarkAsDeleted == routeNodeShadowTable.MarkAsDeleted && routeNode.GetGeoJsonCoordinate() == routeNodeShadowTable.GetGeoJsonCoordinate();

        private bool IsCreatedByApplication(RouteNode routeNode)
            => routeNode.ApplicationName == _applicationSettings.ApplicationName;

        private bool IsModifiedDistanceLessThanTolerance(RouteNode shadowTableNode, RouteNode after)
            => after.GetPoint().Distance(shadowTableNode.GetPoint()) <= _applicationSettings.Tolerance;
    }
}
