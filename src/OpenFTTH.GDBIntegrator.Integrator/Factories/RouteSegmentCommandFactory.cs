using MediatR;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentCommandFactory : IRouteSegmentCommandFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IRouteSegmentValidator _routeSegmentValidator;
        private readonly IGeoDatabase _geoDatabase;
        private readonly IRouteNodeFactory _routeNodeFactory;

        public RouteSegmentCommandFactory(
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

            var (isValid, isValidErrorCode) = _routeSegmentValidator.LineIsValid(after.GetLineString());
            if (!isValid)
            {
                return new List<INotification>
                {
                    new RollbackInvalidRouteSegment(
                        rollbackToSegment: routeSegmentShadowTableBeforeUpdate,
                        message: $"The line string is invalid '{after.GetGeoJsonCoordinate()}'.",
                        errorCode: isValidErrorCode,
                        username: after.Username)
                };
            }

            await _geoDatabase.UpdateRouteSegmentShadowTable(after);

            if (after.MarkAsDeleted)
                return new List<INotification> { CreateRouteSegmentDeleted(after) };

            var intersectingStartSegments = await _geoDatabase.GetIntersectingStartRouteSegments(after);
            var intersectingEndSegments = await _geoDatabase.GetIntersectingEndRouteSegments(after);
            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(after);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(after);
            var allIntersectingRouteNodesNoEdges = await _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(after);

            if (intersectingStartNodes.Count >= 2 || intersectingEndNodes.Count >= 2)
                throw new Exception("Has more than 2 intersecting start or end nodes.");

            if (await IsGeometryChanged(intersectingStartNodes.FirstOrDefault(), intersectingEndNodes.FirstOrDefault(), routeSegmentShadowTableBeforeUpdate))
            {
                var events = new List<INotification>();
                events.Add(new RouteSegmentLocationChanged { RouteSegment = after });

                if (allIntersectingRouteNodesNoEdges.Count > 0)
                {
                    foreach (var intersectingRouteNode in allIntersectingRouteNodesNoEdges)
                    {
                        var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(null, intersectingRouteNode, false);
                        events.Add(routeSegmentSplitted);
                    }
                }

                return events;
            }

            var notifications = new List<INotification>();
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingStartSegments.Count, intersectingStartNodes.Count, after.FindStartPoint(), after));
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingEndSegments.Count, intersectingEndNodes.Count, after.FindEndPoint(), after));

            notifications.Add(new RouteSegmentConnectivityChanged(before, after));

            return notifications;
        }

        public async Task<bool> IsGeometryChanged(RouteNode startNode, RouteNode endNode, RouteSegment beforeShadowTable)
        {
            if (startNode is null || endNode is null)
                return false;

            var previousStartNode = (await _geoDatabase.GetIntersectingStartRouteNodes(beforeShadowTable.Coord)).First();
            var previousEndNode = (await _geoDatabase.GetIntersectingEndRouteNodes(beforeShadowTable.Coord)).First();

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

            // If we find the route segment is in the shadow table, it means that it was created by the application and we therefore do nothing.
            if (await _geoDatabase.RouteSegmentInShadowTableExists(routeSegment.Mrid))
            {
                return new List<INotification>();
            }

            // Update integrator "shadow table" with the used digitized segment
            await _geoDatabase.InsertRouteSegmentShadowTable(routeSegment);

            var (isValid, isValidErrorCode) = _routeSegmentValidator.LineIsValid(routeSegment.GetLineString());
            if (!isValid)
            {
                return new List<INotification>
                {
                    new InvalidRouteSegmentOperation(
                        routeSegment: routeSegment,
                        message: $"The line string is invalid '{routeSegment.GetGeoJsonCoordinate()}'.",
                        errorCode: isValidErrorCode,
                        username: routeSegment.Username
                    )
                };
            }

            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);
            var intersectingStartSegments = await _geoDatabase.GetIntersectingStartRouteSegments(routeSegment);
            var intersectingEndSegments = await _geoDatabase.GetIntersectingEndRouteSegments(routeSegment);
            var allIntersectingRouteNodesNoEdges = await _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment);

            if (intersectingStartNodes.Count >= 2 || intersectingEndNodes.Count >= 2)
            {
                return new List<INotification>
                {
                    new InvalidRouteSegmentOperation(
                        routeSegment: routeSegment,
                        message: "The line string intersects with two or more start or end nodes.",
                        errorCode: ErrorCode.ROUTE_SEGMENT_INTERSECTS_WITH_MULTIPLE_START_OR_END_ROUTE_NODES,
                        username: routeSegment.Username
                    )
                };
            }

            var notifications = new List<INotification>();

            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingStartSegments.Count, intersectingStartNodes.Count, routeSegment.FindStartPoint(), routeSegment));
            notifications.AddRange(HandleExistingRouteSegmentSplitted(intersectingEndSegments.Count, intersectingEndNodes.Count, routeSegment.FindEndPoint(), routeSegment));

            notifications.Add(CreateNewRouteSegmentDigitized(routeSegment));

            if (allIntersectingRouteNodesNoEdges.Count > 0)
            {
                foreach (var intersectingRouteNode in allIntersectingRouteNodesNoEdges)
                {
                    var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(null, intersectingRouteNode, false);
                    notifications.Add(routeSegmentSplitted);
                }
            }

            return notifications;
        }

        private List<INotification> HandleExistingRouteSegmentSplitted(int intersectingSegmentsCount, int intersectingNodesCount, Point point, RouteSegment routeSegment)
        {
            var notifications = new List<INotification>();

            if (intersectingSegmentsCount == 1 && intersectingNodesCount == 0)
            {
                var node = _routeNodeFactory.Create(point);

                var routeSegmentSplitted = CreateExistingRouteSegmentSplitted(routeSegment, node, true);
                notifications.Add(routeSegmentSplitted);
            }

            return notifications;
        }

        private bool AlreadyUpdated(RouteSegment routeSegment, RouteSegment shadowTableRouteSegment)
        {
            return routeSegment.MarkAsDeleted == shadowTableRouteSegment.MarkAsDeleted && routeSegment.GetGeoJsonCoordinate() == shadowTableRouteSegment.GetGeoJsonCoordinate();
        }

        private RouteSegmentDeleted CreateRouteSegmentDeleted(RouteSegment routeSegment)
        {
            return new RouteSegmentDeleted
            {
                RouteSegment = routeSegment,
            };
        }

        private ExistingRouteSegmentSplitted CreateExistingRouteSegmentSplitted(RouteSegment routeSegment, RouteNode routeNode, bool insertRouteNode)
        {
            return new ExistingRouteSegmentSplitted
            {
                RouteNode = routeNode,
                RouteSegmentDigitizedByUser = routeSegment,
                InsertNode = insertRouteNode
            };
        }

        private NewRouteSegmentDigitized CreateNewRouteSegmentDigitized(RouteSegment routeSegment)
        {
            return new NewRouteSegmentDigitized
            {
                RouteSegment = routeSegment,
            };
        }
    }
}
