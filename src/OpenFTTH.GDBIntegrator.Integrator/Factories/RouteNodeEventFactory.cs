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
    public class RouteNodeEventFactory : IRouteNodeEventFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeEventFactory(
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

            var cmdId = Guid.NewGuid();
            if (after.MarkAsDeleted)
                return new List<INotification> { new RouteNodeDeleted { CmdId = cmdId, RouteNode = after, IsLastEventInCmd = true } };

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);
            if (intersectingRouteSegments.Count > 0)
            {
                var notifications = new List<INotification>();
                notifications.Add(new RouteNodeLocationChanged { CmdId = cmdId, RouteNodeAfter = after, RouteNodeBefore = before });

                var previousIntersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(before.Coord);
                var intersectingSegments = (await _geoDatabase.GetIntersectingRouteSegments(after))
                    .Where(x => !previousIntersectingRouteSegments.Any(y => y.Mrid == x.Mrid)).ToList();
                if (intersectingSegments.Count > 0)
                {
                    notifications.Add(new ExistingRouteSegmentSplitted
                    {
                        RouteNode = after,
                        CmdId = cmdId
                    });
                }

                return notifications;
            }

            return new List<INotification> { new RouteNodeLocationChanged { CmdId = cmdId, RouteNodeAfter = after, RouteNodeBefore = before } };
        }

        public async Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            if (IsCreatedByApplication(routeNode))
                return new List<INotification> { new DoNothing($"{nameof(RouteNode)} with id: '{routeNode.Mrid}' was created by nothing therefore do nothing.") };

            await _geoDatabase.InsertRouteNodeShadowTable(routeNode);

            var cmdId = Guid.NewGuid();
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(routeNode);

            if (intersectingRouteNodes.Count > 0)
                return new List<INotification> { new InvalidRouteNodeOperation { RouteNode = routeNode, CmdId = cmdId } };

            if (intersectingRouteSegments.Count == 0)
                return new List<INotification> { new RouteNodeAdded { CmdId = cmdId, RouteNode = routeNode, IsLastEventInCmd = true, CmdType = nameof(NewRouteNodeDigitized) } };

            if (intersectingRouteSegments.Count == 1)
            {
                var notifications = new List<INotification>();
                notifications.Add(new RouteNodeAdded
                {
                    CmdId = cmdId,
                    RouteNode = routeNode,
                    CmdType = nameof(ExistingRouteSegmentSplitted)
                });

                notifications.Add(new ExistingRouteSegmentSplitted
                {
                    RouteNode = routeNode,
                    CmdId = cmdId
                });

                return notifications;
            }

            return new List<INotification> { new InvalidRouteNodeOperation { RouteNode = routeNode, CmdId = cmdId } };
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
