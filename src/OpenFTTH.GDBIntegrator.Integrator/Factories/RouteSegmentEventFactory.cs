using Microsoft.Extensions.Options;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentEventFactory : IRouteSegmentEventFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IModifiedGeometriesStore _modifiedGeometries;

        public RouteSegmentEventFactory(IOptions<ApplicationSetting> applicationSettings, IModifiedGeometriesStore modifiedGeomitriesStore)
        {
            _applicationSettings = applicationSettings.Value;
            _modifiedGeometries = modifiedGeomitriesStore;
        }

        public RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments, bool useApplicationName = false)
        {
            _modifiedGeometries.InsertRouteSegment(routeSegment);

            return new Events.RouteNetwork.RouteSegmentRemoved(
                nameof(Events.RouteNetwork.RouteSegmentRemoved),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                replacedBySegments.ToArray());
        }

        public RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment, bool useApplicationName = false)
        {
            _modifiedGeometries.InsertRouteSegment(routeSegment);

            return new RouteSegmentGeometryModified(
                nameof(RouteSegmentGeometryModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                routeSegment.GetGeoJsonCoordinate(false));
        }

        public RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment, bool useApplicationName = false)
        {
            _modifiedGeometries.InsertRouteSegment(routeSegment);

            return new RouteSegmentMarkedForDeletion(
                nameof(RouteSegmentMarkedForDeletion),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment.ApplicationInfo,
                routeSegment.Mrid);
        }

        public RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode)
        {
            _modifiedGeometries.InsertRouteSegment(routeSegment);

            return new RouteSegmentAdded(
                nameof(Events.RouteNetwork.RouteSegmentAdded),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment?.NamingInfo,
                routeSegment?.LifeCycleInfo,
                routeSegment?.MappingInfo,
                routeSegment?.SafetyInfo,
                routeSegment.Mrid,
                startRouteNode.Mrid,
                endRouteNode.Mrid,
                routeSegment.GetGeoJsonCoordinate(false),
                routeSegment?.RouteSegmentInfo);
        }
    }
}
