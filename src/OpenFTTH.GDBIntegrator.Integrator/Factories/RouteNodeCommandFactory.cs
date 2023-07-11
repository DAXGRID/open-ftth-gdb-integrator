using MediatR;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
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
        private readonly IRouteNodeValidator _routeNodeValidator;

        public RouteNodeCommandFactory(
            IOptions<ApplicationSetting> applicationSettings,
            IGeoDatabase geoDatabase,
            IRouteNodeValidator routeNodeValidator)
        {
            _applicationSettings = applicationSettings.Value;
            _geoDatabase = geoDatabase;
            _routeNodeValidator = routeNodeValidator;
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

            if (!_routeNodeValidator.PointIsValid(after.GetPoint()))
                throw new ArgumentException("Point is not valid.");

            if (IsModifiedDistanceLessThanTolerance(shadowTableNode, after))
            {
                return new List<INotification>
                {
                    new RollbackInvalidRouteNode(
                        rollbackToNode: shadowTableNode,
                        message: "Route node's distance was modified distance less than tolerance.",
                        errorCode: ErrorCode.ROUTE_NODE_MODIFIED_LESS_THAN_TOLERANCE,
                        username: after.Username
                    )
                };
            }

            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(after);
            if (intersectingRouteNodes.Count > 0)
            {
                return new List<INotification>
                {
                    new RollbackInvalidRouteNode(
                        rollbackToNode: shadowTableNode,
                        message: "The route node intersects with another route node.",
                        errorCode: ErrorCode.ROUTE_NODE_INTERSECTS_WITH_ANOTHER_ROUTE_NODE,
                        username: after.Username
                    )
                };
            }

            if (IsMarkedToBeDeletedAndGeometryChanged(shadowTableNode, after))
            {
                return new List<INotification>
                {
                    new RollbackInvalidRouteNode(
                        rollbackToNode: shadowTableNode,
                        message: "Modifying the geometry and marking the route node to be deleted in the same operation is not valid.",
                        errorCode: ErrorCode.ROUTE_NODE_CANNOT_MODIFY_GEOMETRY_AND_MARK_FOR_DELETION_IN_THE_SAME_OPERATION,
                        username: after.Username
                    )
                };
            }

            if (await BeforeValueIntersectsWithRouteSegmentAndAfterIsMarkedToBeDeleted(shadowTableNode, after))
            {
                return new List<INotification>
                {
                    new RollbackInvalidRouteNode(
                        rollbackToNode: shadowTableNode,
                        message: "Route node that intersects with route segment cannot be marked to deleted.",
                        errorCode: ErrorCode.ROUTE_NODE_INTERSECT_WITH_ROUTE_SEGMENT_CANNOT_BE_DELETED,
                        username: after.Username
                    )
                };
            }

            await _geoDatabase.UpdateRouteNodeShadowTable(after);

            // We roll back in-case the update-command intersects with new route-segments
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);
            if (intersectingRouteSegments.Count > 0)
            {
                var previousIntersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(shadowTableNode.Coord);
                var newIntersectingRouteSegments = intersectingRouteSegments
                    .Where(x => !previousIntersectingRouteSegments.Any(y => y.Mrid == x.Mrid)).ToList();
                if (newIntersectingRouteSegments.Count > 0)
                {
                    return new List<INotification>
                    {
                        new RollbackInvalidRouteNode(
                            shadowTableNode,
                            "It is not allowed to change the geometry of a route node so it intersects with one or more route segments.",
                            errorCode: ErrorCode.ROUTE_NODE_GEOMETRY_UPDATE_NOT_ALLOWED_TO_INTERSECT_WITH_ROUTE_SEGMENT,
                            username: after.Username
                        )
                    };
                }
            }

            if (after.MarkAsDeleted)
            {
                return new List<INotification> { new RouteNodeDeleted { RouteNode = after } };
            }

            return new List<INotification> { new RouteNodeLocationChanged { RouteNodeAfter = after, RouteNodeBefore = shadowTableNode } };
        }

        public async Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
            {
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null.");
            }

            // If we find the route node is in the shadow table, it means that it was created by the application and we therefore do nothing.
            if (await _geoDatabase.RouteNodeInShadowTableExists(routeNode.Mrid))
            {
                return new List<INotification>
                {
                    new DoNothing($"{nameof(RouteNode)} with id: '{routeNode.Mrid}' was created by {routeNode.ApplicationName} therefore do nothing.")
                };
            }

            await _geoDatabase.InsertRouteNodeShadowTable(routeNode);

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);
            var intersectingRouteNodes = await _geoDatabase.GetIntersectingRouteNodes(routeNode);

            if (intersectingRouteNodes.Count > 0)
            {
                return new List<INotification>
                {
                    new InvalidRouteNodeOperation(
                        routeNode: routeNode,
                        message: "The route node intersects with another route node.",
                        errorCode: ErrorCode.ROUTE_NODE_INTERSECTS_WITH_ANOTHER_ROUTE_NODE,
                        username: routeNode.Username
                    )
                };
            }

            if (intersectingRouteSegments.Count == 0)
            {
                return new List<INotification> { new NewRouteNodeDigitized { RouteNode = routeNode } };
            }

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

            return new List<INotification>
            {
                new InvalidRouteNodeOperation(
                    routeNode: routeNode,
                    message: "Route node did not fit any condition in command factory.",
                    errorCode: ErrorCode.UNKNOWN_ERROR,
                    username: routeNode.Username
                )
            };
        }

        private bool IsMarkedToBeDeletedAndGeometryChanged(RouteNode shadowTableNode, RouteNode after)
        {
            return after.MarkAsDeleted && !after.GetPoint().EqualsTopologically(shadowTableNode.GetPoint());
        }

        private async Task<bool> BeforeValueIntersectsWithRouteSegmentAndAfterIsMarkedToBeDeleted(RouteNode shadowTableNode, RouteNode after)
        {
            var startRouteSegment = await _geoDatabase.GetIntersectingStartRouteSegments(shadowTableNode);
            var endRouteSegment = await _geoDatabase.GetIntersectingEndRouteSegments(shadowTableNode);
            return (startRouteSegment.Count + endRouteSegment.Count) > 0 && after.MarkAsDeleted;
        }

        private bool AlreadyUpdated(RouteNode routeNode, RouteNode routeNodeShadowTable)
            => routeNode.MarkAsDeleted == routeNodeShadowTable.MarkAsDeleted && routeNode.GetGeoJsonCoordinate() == routeNodeShadowTable.GetGeoJsonCoordinate();

        private bool IsModifiedDistanceLessThanTolerance(RouteNode shadowTableNode, RouteNode after)
        {
            var distance = after.GetPoint().Distance(shadowTableNode.GetPoint());
            return distance != 0 && distance <= _applicationSettings.Tolerance;
        }
    }
}
