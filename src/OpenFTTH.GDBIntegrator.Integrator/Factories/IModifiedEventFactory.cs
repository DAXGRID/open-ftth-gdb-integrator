using OpenFTTH.Events.Core;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IModifiedEventFactory
    {
        RouteSegmentInfoModified CreateRouteSegmentInfoModified(RouteSegment routeSegment);
        RouteNodeInfoModified CreateRouteNodeInfoModified(RouteNode routeNode);
        LifecycleInfoModified CreateLifeCycleInfoModified(RouteSegment routeSegment);
        LifecycleInfoModified CreateLifeCycleInfoModified(RouteNode routeNode);
    }
}
