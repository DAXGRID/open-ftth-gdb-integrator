using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.GeoDatabase
{
    public interface IGeoDatabase
    {
        Task<List<RouteNode>> GetIntersectingRouteNodes(RouteSegment routeSegment);
        Task InsertRouteNode(RouteNode routeNode);
    }
}
