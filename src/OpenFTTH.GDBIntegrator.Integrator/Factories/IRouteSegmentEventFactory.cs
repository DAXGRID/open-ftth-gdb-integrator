using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentEventFactory
    {
        public RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments);
        public RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment);
        public RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment);
        public RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode);
    }
}
