using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.GeoDatabase
{
    public interface IGeoDatabase
    {
        Task<RouteNode> GetRouteNodeShadowTable(Guid mrid);
        Task<RouteSegment> GetRouteSegmentShadowTable(Guid mrid);
        Task<List<RouteNode>> GetIntersectingStartRouteNodes(RouteSegment routeSegment);
        Task<List<RouteNode>> GetIntersectingStartRouteNodes(byte[] coord);
        Task<List<RouteNode>> GetIntersectingEndRouteNodes(RouteSegment routeSegment);
        Task<List<RouteNode>> GetIntersectingEndRouteNodes(byte[] coord);
        Task<List<RouteNode>> GetAllIntersectingRouteNodes(RouteSegment routeSegment);
        Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(RouteSegment routeSegment);
        Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(byte[] coordinates, Guid mrid);
        Task<List<RouteNode>> GetIntersectingRouteNodes(RouteNode routeNode);
        Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode);
        Task<List<RouteSegment>> GetIntersectingRouteSegments(byte[] coordinates);
        Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode, RouteSegment notInclude);
        Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteSegment routeSegment);
        Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteNode routeNode);
        Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteSegment routeSegment);
        Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteNode routeNode);
        Task DeleteRouteNode(Guid mrid);
        Task DeleteRouteSegment(Guid mrid);
        Task MarkDeleteRouteSegment(Guid mrid);
        Task MarkDeleteRouteNode(Guid mrid);
        Task InsertRouteNode(RouteNode routeNode);
        Task UpdateRouteNode(RouteNode routeNode);
        Task InsertRouteNodeShadowTable(RouteNode routeNode);
        Task UpdateRouteNodeShadowTable(RouteNode routeNode);
        Task InsertRouteSegment(RouteSegment routeSegment);
        Task UpdateRouteSegment(RouteSegment routeSegment);
        Task InsertRouteSegmentShadowTable(RouteSegment routeSegment);
        Task UpdateRouteSegmentShadowTable(RouteSegment routeSegment);
        Task<string> GetRouteSegmentsSplittedByRouteNode(RouteNode routeNode, RouteSegment intersectingRouteSegment);
    }
}
