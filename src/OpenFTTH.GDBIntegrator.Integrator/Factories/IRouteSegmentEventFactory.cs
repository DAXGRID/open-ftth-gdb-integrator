using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentEventFactory
    {
        RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments);
        RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment);
        RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment);
        RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode);
    }
}
