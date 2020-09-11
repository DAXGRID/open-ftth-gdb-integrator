using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface RouteSegmentEventFactory
    {
        public RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments)
        {
            return new Events.RouteNetwork.RouteSegmentRemoved(
                nameof(Events.RouteNetwork.RouteSegmentRemoved),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                replacedBySegments.ToArray());
        }

        public RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment)
        {
            return new RouteSegmentGeometryModified(
                nameof(RouteSegmentGeometryModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                routeSegment.GetGeoJsonCoordinate());
        }

        public RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment, string cmdType)
        {
            return new RouteSegmentMarkedForDeletion(
                nameof(RouteSegmentMarkedForDeletion),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment?.ApplicationName,
                routeSegment.ApplicationInfo,
                routeSegment.Mrid);
        }

        public RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode)
        {
            return new Events.RouteNetwork.RouteSegmentAdded(
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
                routeSegment.GetGeoJsonCoordinate(),
                routeSegment?.RouteSegmentInfo);
        }
    }
}
