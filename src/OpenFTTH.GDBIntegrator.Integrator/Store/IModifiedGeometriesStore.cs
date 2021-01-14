using OpenFTTH.GDBIntegrator.RouteNetwork;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public interface IModifiedGeometriesStore
    {
        void InsertRouteSegment(RouteSegment routeSegment);
        void InsertRouteNode(RouteNode routeNode);
        List<RouteSegment> GetRouteSegments();
        List<RouteNode> GetRouteNodes();
        void Clear();
    }
}
