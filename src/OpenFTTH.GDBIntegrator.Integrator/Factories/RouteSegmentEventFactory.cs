using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
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

        public RouteSegmentEventFactory(
            IOptions<ApplicationSetting> applicationSettings,
            IRouteSegmentValidator routeSegmentValidator,
            IGeoDatabase geoDatabase)
        {
            _applicationSettings = applicationSettings.Value;
            _routeSegmentValidator = routeSegmentValidator;
            _geoDatabase = geoDatabase;
        }

        public async Task<IEnumerable<INotification>> Create(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"Parameter {nameof(routeSegment)} must not be null");

            if (IsCreatedByApplication(routeSegment))
                return new List<INotification>();

            // Update integrator "shadow table" with the used digitized segment
            await _geoDatabase.InsertRouteSegmentIntegrator(routeSegment);

            var eventId = Guid.NewGuid();

            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new List<INotification> { new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId } };

            var intersectingStartNodesTask = _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodesTask = _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);
            var intersectingStartSegmentsTask = _geoDatabase.GetIntersectingStartRouteSegments(routeSegment);
            var intersectingEndSegmentsTask = _geoDatabase.GetIntersectingEndRouteSegments(routeSegment);
            var allIntersectingRouteNodesTask = _geoDatabase.GetAllIntersectingRouteNodesNotIncludedEdges(routeSegment);

            var intersectingStartNodes = await intersectingStartNodesTask;
            var intersectingEndNodes = await intersectingEndNodesTask;
            var intersectingStartSegments = await intersectingStartSegmentsTask;
            var intersectingEndSegments = await intersectingEndSegmentsTask;
            var allIntersectingRouteNodes = await allIntersectingRouteNodesTask;

            var notifications = new List<INotification>();

            if (allIntersectingRouteNodes.Count > 0)
            {
                notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, eventId));
                foreach (var intersectingRouteNode in allIntersectingRouteNodes)
                {
                    var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(null, eventId, intersectingRouteNode);
                    notifications.Add(routeSegmentSplitted);
                }

                return notifications;
            }

            if (intersectingStartSegments.Count == 1 && intersectingStartNodes.Count == 0)
            {
                var startNode = routeSegment.FindStartNode();
                notifications.Add(new InsertRouteNode { EventId = eventId, RouteNode = startNode });

                var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(routeSegment, eventId, startNode);
                notifications.Add(routeSegmentSplitted);
            }

            if (intersectingEndSegments.Count == 1 && intersectingEndNodes.Count == 0)
            {
                var endNode = routeSegment.FindEndNode();
                notifications.Add(new InsertRouteNode { EventId = eventId, RouteNode = endNode });

                var routeSegmentSplitted = CreateExistingRouteSegmentSplittedByUser(routeSegment, eventId, endNode);
                notifications.Add(routeSegmentSplitted);
            }

            notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, eventId));

            return notifications;
        }

        private bool IsCreatedByApplication(RouteSegment routeSegment)
        {
            return routeSegment.ApplicationName == _applicationSettings.ApplicationName;
        }

        private ExistingRouteSegmentSplittedByUser CreateExistingRouteSegmentSplittedByUser(RouteSegment routeSegment, Guid eventId, RouteNode routeNode)
        {
            return new ExistingRouteSegmentSplittedByUser
            {
                RouteNode = routeNode,
                EventId = eventId,
                RouteSegmentDigitizedByUser = routeSegment,
            };
        }

        private NewRouteSegmentDigitizedByUser CreateNewRouteSegmentDigitizedByUser(RouteSegment routeSegment, Guid eventId)
        {
            return new NewRouteSegmentDigitizedByUser
            {
                RouteSegment = routeSegment,
                EventId = eventId
            };
        }
    }
}
