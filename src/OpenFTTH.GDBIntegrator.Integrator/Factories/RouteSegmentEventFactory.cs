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

            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);
            var intersectingStartSegments = await _geoDatabase.GetIntersectingStartRouteSegments(routeSegment);
            var intersectingEndSegments = await _geoDatabase.GetIntersectingEndRouteSegments(routeSegment);

            var notifications = new List<INotification>();

            if (intersectingStartSegments.Count == 1 && intersectingStartNodes.Count == 0)
            {
                var intersectingStart = intersectingStartSegments.FirstOrDefault();
                var intersectingStartIntersectionStart = (await _geoDatabase.GetIntersectingStartRouteSegments(intersectingStart)).FirstOrDefault();
                var intersectingStartIntersectionEnd = (await _geoDatabase.GetIntersectingEndRouteSegments(intersectingStart)).FirstOrDefault();

                if ((intersectingStartIntersectionStart is null || intersectingStartIntersectionStart.Mrid != routeSegment.Mrid)
                    && (intersectingStartIntersectionEnd is null || intersectingStartIntersectionEnd.Mrid != routeSegment.Mrid))
                    notifications.Add(CreateExistingRouteSegmentSplittedByUser(routeSegment, eventId, routeSegment.FindStartNode()));
            }

            if (intersectingEndSegments.Count == 1 && intersectingEndNodes.Count == 0)
            {
                var intersectingEnd = intersectingEndSegments.FirstOrDefault();
                var intersectingEndIntersectionStart = (await _geoDatabase.GetIntersectingStartRouteSegments(intersectingEnd)).FirstOrDefault();
                var intersectingEndIntersectionEnd = (await _geoDatabase.GetIntersectingEndRouteSegments(intersectingEnd)).FirstOrDefault();
                if ((intersectingEndIntersectionStart is null || intersectingEndIntersectionStart.Mrid != routeSegment.Mrid)
                    && (intersectingEndIntersectionEnd is null || intersectingEndIntersectionEnd.Mrid != routeSegment.Mrid))
                    notifications.Add(CreateExistingRouteSegmentSplittedByUser(routeSegment, eventId, routeSegment.FindEndNode()));
            }

            notifications.Add(CreateNewRouteSegmentDigitizedByUser(routeSegment, eventId));

            return notifications;
        }

        private INotification CreateNewRouteSegmentDigitizedByUser(RouteSegment routeSegment, Guid eventId)
        {
            return new NewRouteSegmentDigitizedByUser
            {
                RouteSegment = routeSegment,
                EventId = eventId
            };
        }

        private INotification CreateExistingRouteSegmentSplittedByUser(RouteSegment routeSegment, Guid eventId, RouteNode routeNode)
        {
            return new ExistingRouteSegmentSplittedByUser
            {
                RouteNode = routeNode,
                EventId = eventId,
                InsertRouteNode = true,
                RouteSegmentDigitizedByUser = routeSegment
            };
        }
    }
}
