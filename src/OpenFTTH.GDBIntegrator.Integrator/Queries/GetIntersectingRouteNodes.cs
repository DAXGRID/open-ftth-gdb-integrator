using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Queries
{
    public class GetIntersectingRouteNodes : IRequest<List<RouteNode>>
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class GetIntersectingRouteNodesHandler : IRequestHandler<GetIntersectingRouteNodes, List<RouteNode>>
    {
        private readonly IGeoDatabase _geoDatabase;

        public GetIntersectingRouteNodesHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<List<RouteNode>> Handle(GetIntersectingRouteNodes request, CancellationToken cancellationToken)
        {
            var routeNodes = await _geoDatabase.GetIntersectingRouteNodes(request.RouteSegment);
            return routeNodes;
        }
    }
}
