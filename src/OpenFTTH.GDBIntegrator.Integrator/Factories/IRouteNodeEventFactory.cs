using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeEventFactory
    {
        public RouteNodeAdded CreateAdded(RouteNode routeNode);
        public RouteNodeMarkedForDeletion CreateMarkedForDeletion(RouteNode routeNode);
        public RouteNodeGeometryModified CreateGeometryModified(RouteNode routeNode);
    }
}
