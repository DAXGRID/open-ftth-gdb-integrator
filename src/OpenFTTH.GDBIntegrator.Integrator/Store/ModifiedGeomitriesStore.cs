using OpenFTTH.GDBIntegrator.RouteNetwork;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public class ModifiedGeomitriesStore : IModifiedGeomitriesStore
    {
        private List<RouteSegment> _routeSegments = new List<RouteSegment>();
        private List<RouteNode> _routeNodes = new List<RouteNode>();

        public void InsertRouteSegment(RouteSegment routeSegment)
        {
            _routeSegments.Add(routeSegment);
        }

        public void InsertRouteNode(RouteNode routeNode)
        {
            _routeNodes.Add(routeNode);
        }

        public List<RouteSegment> GetRouteSegments()
        {
            return _routeSegments;
        }

        public List<RouteNode> GetRouteNodes()
        {
            return _routeNodes;
        }

        public void Clear()
        {
            _routeNodes.Clear();
            _routeSegments.Clear();
        }
    }
}
