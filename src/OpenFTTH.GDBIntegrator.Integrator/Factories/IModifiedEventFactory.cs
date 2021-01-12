using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IModifiedEventFactory
    {
        RouteSegmentInfoModified CreateRouteSegmentInfoModified(RouteSegment routeSegment);
    }
}
