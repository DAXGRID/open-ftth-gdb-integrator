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

            // Do nothing created by application
            if (routeSegment.ApplicationName == _applicationSettings.ApplicationName)
                return new List<INotification>();

            var eventId = Guid.NewGuid();

            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new List<INotification> { new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId } };

            var intersectingStartNodesTask = _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodesTask = _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);
            var intersectingStartSegmentsTask = _geoDatabase.GetIntersectingStartRouteSegments(routeSegment);
            var intersectingEndSegmentsTask = _geoDatabase.GetIntersectingEndRouteSegments(routeSegment);

            await Task.WhenAll(intersectingEndSegmentsTask, intersectingEndNodesTask, intersectingStartNodesTask, intersectingStartSegmentsTask);

            var intersectingStartNodes = intersectingStartNodesTask.Result;
            var intersectingEndNodes = intersectingEndNodesTask.Result;
            var intersectingStartSegments = intersectingStartSegmentsTask.Result;
            var intersectingEndSegments = intersectingEndSegmentsTask.Result;

            var notifications = new List<INotification>();

            if (intersectingStartSegments.Count == 1 && intersectingStartNodes.Count == 0)
            {
                var routeSegmentSplitted = await CreateExistingRouteSegmentSplittedByUser(intersectingStartSegments, routeSegment, eventId, routeSegment.FindStartNode());
                if (!(routeSegmentSplitted is null))
                    notifications.Add(routeSegmentSplitted);
            }

            if (intersectingEndSegments.Count == 1 && intersectingEndNodes.Count == 0)
            {
                var routeSegmentSplitted = await CreateExistingRouteSegmentSplittedByUser(intersectingEndSegments, routeSegment, eventId, routeSegment.FindEndNode());
                if (!(routeSegmentSplitted is null))
                    notifications.Add(routeSegmentSplitted);
            }

            notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, eventId));

            return notifications;
        }

        private async Task<INotification> CreateExistingRouteSegmentSplittedByUser(List<RouteSegment> intersectingSegments, RouteSegment routeSegment, Guid eventId, RouteNode routeNode)
        {
            var intersectings = intersectingSegments.FirstOrDefault();
            var intersectionsStart = (await _geoDatabase.GetIntersectingStartRouteSegments(intersectings)).FirstOrDefault();
            var intersectionsEnd = (await _geoDatabase.GetIntersectingEndRouteSegments(intersectings)).FirstOrDefault();

            var isIntersectingStartPoint = (intersectionsStart is null || intersectionsStart.Mrid != routeSegment.Mrid);
            var isIntersectingEndPoint = (intersectionsEnd is null || intersectionsEnd.Mrid != routeSegment.Mrid);

            if (isIntersectingStartPoint && isIntersectingEndPoint)
            {
                return new ExistingRouteSegmentSplittedByUser
                {
                    RouteNode = routeNode,
                    EventId = eventId,
                    InsertRouteNode = true,
                    RouteSegmentDigitizedByUser = routeSegment
                };
            }
            else
            {
                return null;
            }
        }


        private INotification CreateNewRouteSegmentDigitizedByUser(RouteSegment routeSegment, Guid eventId)
        {
            return new NewRouteSegmentDigitizedByUser
            {
                RouteSegment = routeSegment,
                EventId = eventId
            };
        }
    }
}
