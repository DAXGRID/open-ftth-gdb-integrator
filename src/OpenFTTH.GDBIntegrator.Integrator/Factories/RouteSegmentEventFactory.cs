using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Options;
using MediatR;

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

        public async Task<INotification> CreateUpdatedEvent(RouteSegment before, RouteSegment after)
        {
            await _geoDatabase.UpdateRouteSegmentShadowTable(after);
            var cmdId = Guid.NewGuid();
            if (after.MarkAsDeleted)
            {
                return new RouteSegmentDeleted
                {
                    RouteSegment = after,
                    CmdId = cmdId
                };
            }

            return new InvalidRouteSegmentOperation();
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
            var allIntersectingRouteNodesTask = _geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment);

            var intersectingStartNodes = await intersectingStartNodesTask;
            var intersectingEndNodes = await intersectingEndNodesTask;
            var intersectingStartSegments = await intersectingStartSegmentsTask;
            var intersectingEndSegments = await intersectingEndSegmentsTask;
            var allIntersectingRouteNodes = await allIntersectingRouteNodesTask;

            var notifications = new List<INotification>();

            if (allIntersectingRouteNodes.Count > 0)
            {
                notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, cmdId));
                foreach (var intersectingRouteNode in allIntersectingRouteNodes)
                {
                    var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(null, cmdId, intersectingRouteNode);
                    notifications.Add(routeSegmentSplitted);
                }

                return notifications;
            }

            if (intersectingStartSegments.Count == 1 && intersectingStartNodes.Count == 0)
            {
                var startPoint = routeSegment.FindStartPoint();
                var startNode = _routeNodeFactory.Create(startPoint);
                notifications.Add(new NewRouteNodeDigitized { CmdId = cmdId, RouteNode = startNode });

                var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(routeSegment, cmdId, startNode);
                notifications.Add(routeSegmentSplitted);
            }

            if (intersectingEndSegments.Count == 1 && intersectingEndNodes.Count == 0)
            {
                var endPoint = routeSegment.FindEndPoint();
                var endNode = _routeNodeFactory.Create(endPoint);
                notifications.Add(new NewRouteNodeDigitized { CmdId = cmdId, RouteNode = endNode });

                var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(routeSegment, cmdId, endNode);
                notifications.Add(routeSegmentSplitted);
            }

            notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, cmdId));

            return notifications;
        }

        private bool IsCreatedByApplication(RouteSegment routeSegment)
        {
            return routeSegment.ApplicationName == _applicationSettings.ApplicationName;
        }

        private ExistingRouteSegmentSplitted CreateExistingRouteSegmentSplittedByUser(RouteSegment routeSegment, Guid cmdId, RouteNode routeNode)
        {
            return new ExistingRouteSegmentSplitted
            {
                RouteNode = routeNode,
                CmdId = cmdId,
                RouteSegmentDigitizedByUser = routeSegment,
            };
        }

        private NewRouteSegmentDigitized CreateNewRouteSegmentDigitizedByUser(RouteSegment routeSegment, Guid cmdId)
        {
            return new NewRouteSegmentDigitized
            {
                RouteSegment = routeSegment,
                EventId = cmdId
            };
        }
    }
}
