using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.GeoDatabase
{
    public interface IGeoDatabase
    {
        Task<List<RouteNode>> GetIntersectingStartRouteNodes(RouteSegment routeSegment);
        Task<List<RouteNode>> GetIntersectingEndRouteNodes(RouteSegment routeSegment);
        Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode);
        Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteSegment routeSegment);
        Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteSegment routeSegment);
        Task DeleteRouteNode(Guid mrid);
        Task DeleteRouteSegment(Guid mrid);
        Task MarkDeleteRouteSegment(Guid mrid);
        Task MarkDeleteRouteNode(Guid mrid);
        Task InsertRouteNode(RouteNode routeNode);
        Task InsertRouteSegment(RouteSegment routeSegment);
        Task<string> GetRouteSegmentsSplittedByRouteNode(RouteNode routeNode, RouteSegment intersectingRouteSegment);
    }
}
