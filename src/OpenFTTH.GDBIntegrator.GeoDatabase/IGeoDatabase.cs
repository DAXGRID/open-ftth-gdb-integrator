using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.GeoDatabase
{
    public interface IGeoDatabase
    {
        Task<bool> RouteNodeInShadowTableExists(Guid mrid);
        Task<RouteNode> GetRouteNodeShadowTable(Guid mrid, bool includeDeleted = false);
        Task<bool> RouteSegmentInShadowTableExists(Guid mrid);
        Task<RouteSegment> GetRouteSegmentShadowTable(Guid mrid, bool includeDeleted = false);
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
        Task UpdateRouteNodeInfosShadowTable(RouteNode routeNode);
        Task InsertRouteSegment(RouteSegment routeSegment);
        Task UpdateRouteSegment(RouteSegment routeSegment);
        Task InsertRouteSegmentShadowTable(RouteSegment routeSegment);
        Task UpdateRouteSegmentShadowTable(RouteSegment routeSegment);
        Task UpdateRouteSegmentInfosShadowTable(RouteSegment routeSegment);
        Task<string> GetRouteSegmentsSplittedByRouteNode(RouteNode routeNode, RouteSegment intersectingRouteSegment);
        Task BeginTransaction();
        Task DisposeTransaction();
        Task DisposeConnection();
        Task Commit();
        Task RollbackTransaction();
    }
}
