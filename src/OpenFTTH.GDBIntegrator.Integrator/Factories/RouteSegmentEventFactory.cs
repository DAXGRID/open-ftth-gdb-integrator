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

            // Case 5, 6
            if (intersectingStartNodes.Count == 0 && intersectingEndNodes.Count == 0 && intersectingStartSegments.Count == 1 || intersectingEndSegments.Count == 1)
            {
                var notifications = new List<INotification>();

                if (intersectingStartSegments.Count == 1)
                {
                    notifications.Add(new ExistingRouteSegmentSplittedByUser
                    {
                        RouteNode = routeSegment.FindStartNode(),
                        EventId = eventId,
                        InsertRouteNode = true,
                        CreatedRouteSegment = routeSegment
                    });
                }

                if (intersectingEndSegments.Count == 1)
                {
                    notifications.Add(new ExistingRouteSegmentSplittedByUser
                    {
                        RouteNode = routeSegment.FindEndNode(),
                        EventId = eventId,
                        InsertRouteNode = true,
                        CreatedRouteSegment = routeSegment
                    });
                }

                notifications.Add(new NewRouteSegmentDigitizedByUser
                {
                    RouteSegment = routeSegment,
                    EventId = eventId
                });

                return notifications;
            }

            // Case 1-3
            if (intersectingStartNodes.Count <= 1 && intersectingEndNodes.Count <= 1)
            {
                return new List<INotification> { new NewRouteSegmentDigitizedByUser
                {
                    RouteSegment = routeSegment,
                    EventId = eventId
                }};
            }           

            return new List<INotification> { new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId } };
        }
    }
}
