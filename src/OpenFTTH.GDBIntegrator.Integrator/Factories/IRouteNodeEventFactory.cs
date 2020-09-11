using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeEventFactory
    {
        RouteNodeAdded CreateAdded(RouteNode routeNode);
        RouteNodeMarkedForDeletion CreateMarkedForDeletion(RouteNode routeNode);
        RouteNodeGeometryModified CreateGeometryModified(RouteNode routeNode);
    }
}
