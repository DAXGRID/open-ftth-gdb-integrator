using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentEventFactory
    {
        public RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments);
        public RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment, bool useApplicationName = false);
        public RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment, bool useApplicationName = false);
        public RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode);
    }
}
