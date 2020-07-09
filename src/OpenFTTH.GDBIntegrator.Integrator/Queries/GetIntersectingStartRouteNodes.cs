using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Queries
{
    public class GetIntersectingStartRouteNodes : IRequest<List<RouteNode>>
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class GetIntersectingStartRouteNodesHandler : IRequestHandler<GetIntersectingStartRouteNodes, List<RouteNode>>
    {
        private readonly IGeoDatabase _geoDatabase;

        public GetIntersectingStartRouteNodesHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<List<RouteNode>> Handle(GetIntersectingStartRouteNodes request, CancellationToken cancellationToken)
        {
            var routeNodes = await _geoDatabase.GetIntersectingStartRouteNodes(request.RouteSegment);
            return routeNodes;
        }
    }
}
