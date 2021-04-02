using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Options;
using MediatR;

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
                throw new ArgumentNullException($"Parameter {nameof(before)} or {nameof(after)} cannot be null");

            var shadowTableNode = await _geoDatabase.GetRouteNodeShadowTable(after.Mrid);

            if (shadowTableNode is null)
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} is already deleted, so do nothing.") };

            if (AlreadyUpdated(after, shadowTableNode))
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} with id: '{after.Mrid}' was already updated therefore do nothing.") };

            await _geoDatabase.UpdateRouteNodeShadowTable(after);

            if (!(await IsValidNodeUpdate(before, after)))
                return new List<INotification> { new RollbackInvalidRouteNode(before) };

            if (after.MarkAsDeleted)
                return new List<INotification> { new RouteNodeDeleted { RouteNode = after } };

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);
            if (intersectingRouteSegments.Count > 0)
            {
                var notifications = new List<INotification>();
                notifications.Add(new RouteNodeLocationChanged { RouteNodeAfter = after, RouteNodeBefore = before });

                var previousIntersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(before.Coord);
                var intersectingSegments = (await _geoDatabase.GetIntersectingRouteSegments(after))
                    .Where(x => !previousIntersectingRouteSegments.Any(y => y.Mrid == x.Mrid)).ToList();
                if (intersectingSegments.Count > 0)
                {
                    notifications.Add(new ExistingRouteSegmentSplitted
                    {
                        RouteNode = after,
                    });
                }

                return notifications;
            }

            return new List<INotification> { new RouteNodeLocationChanged { RouteNodeAfter = after, RouteNodeBefore = before } };
        }

        public async Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            if (IsCreatedByApplication(routeNode))
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} with id: '{routeNode.Mrid}' was created by nothing therefore do nothing.") };

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

        private async Task<bool> IsValidNodeUpdate(RouteNode before, RouteNode after)
        {
            var startRouteSegment = await _geoDatabase.GetIntersectingStartRouteSegments(before);
            var endRouteSegment = await _geoDatabase.GetIntersectingEndRouteSegments(before);
            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(after);

            if (((startRouteSegment.Count + endRouteSegment.Count) > 0 && after.MarkAsDeleted) || intersectingRouteNodes.Count > 0)
                return false;

            return true;
        }

        private async Task RollbackInvalidOperation(RouteNode rollbackToNode)
        {
            await _geoDatabase.UpdateRouteNode(rollbackToNode);
        }

        private bool AlreadyUpdated(RouteNode routeNode, RouteNode integratorRouteNode)
        {
            return routeNode.MarkAsDeleted == integratorRouteNode.MarkAsDeleted && routeNode.GetGeoJsonCoordinate() == integratorRouteNode.GetGeoJsonCoordinate();
        }

        private bool IsCreatedByApplication(RouteNode routeNode)
        {
            return routeNode.ApplicationName == _applicationSettings.ApplicationName;
        }
    }
}
