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
        MappingInfoModified CreateMappingInfoModified(RouteSegment routeSegment);
        MappingInfoModified CreateMappingInfoModified(RouteNode routeNode);
        SafetyInfoModified CreateSafetyInfoModified(RouteSegment routeSegment);
        SafetyInfoModified CreateSafetyInfoModified(RouteNode routeNode);
        NamingInfoModified CreateNamingInfoModified(RouteSegment routeSegment);
        NamingInfoModified CreateNamingInfoModified(RouteNode routeNode);
    }
}
