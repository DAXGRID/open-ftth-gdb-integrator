using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Options;

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

        public async Task<INotification> Create(RouteSegment routeSegment)
        {
            var eventId = Guid.NewGuid();

            if (routeSegment.ApplicationName == _applicationSettings.ApplicationName)
                return null;

            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId };

            var intersectingStartNodes = await _geoDatabase.GetIntersectingStartRouteNodes(routeSegment);
            var intersectingEndNodes = await _geoDatabase.GetIntersectingEndRouteNodes(routeSegment);

            var totalIntersectingNodes = intersectingStartNodes.Count + intersectingEndNodes.Count;

            if (intersectingStartNodes.Count <= 1 && intersectingEndNodes.Count <= 1)
            {
                return new NewRouteSegmentDigitizedByUser
                {
                    RouteSegment = routeSegment,
                    StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                    EndRouteNode = intersectingEndNodes.FirstOrDefault(),
                    EventId = eventId
                };
            }

            return new InvalidRouteSegmentOperation { RouteSegment = routeSegment, EventId = eventId };
        }
    }
}
