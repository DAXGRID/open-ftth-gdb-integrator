using System;
using System.Threading.Tasks;
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


        public async Task<INotification> CreateUpdatedEvent(RouteNode before, RouteNode after)
        {
            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);

            var eventId = Guid.NewGuid();
            if (after.MarkAsDeleted && intersectingRouteSegments.Count == 0)
                return new RouteNodeDeleted { EventId = eventId, RouteNode = after };

            return null;
        }

        public async Task<INotification> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            if (IsCreatedByApplication(routeNode))
                return null;

            // Update the 'shadow' table
            await _geoDatabase.InsertRouteNodeIntegrator(routeNode);

            var eventId = Guid.NewGuid();
            var intersectingRouteSegmentsTask = _geoDatabase.GetIntersectingRouteSegments(routeNode);
            var intersectingRouteNodesTask = _geoDatabase.GetIntersectingRouteNodes(routeNode);

            var intersectingRouteSegments = await intersectingRouteSegmentsTask;
            var intersectingRouteNodes = await intersectingRouteNodesTask;

            if (intersectingRouteNodes.Count > 0)
                return new InvalidRouteNodeOperation { RouteNode = routeNode, EventId = eventId };

            if (intersectingRouteSegments.Count == 0)
                return new RouteNodeAdded { EventId = eventId, RouteNode = routeNode };

            if (intersectingRouteSegments.Count == 1)
            {
                return new ExistingRouteSegmentSplitted
                {
                    RouteNode = routeNode,
                    EventId = eventId
                };
            }

            return new InvalidRouteNodeOperation { RouteNode = routeNode, EventId = eventId };
        }

        private bool IsCreatedByApplication(RouteNode routeNode)
        {
            return routeNode.ApplicationName == _applicationSettings.ApplicationName;
        }
    }
}
