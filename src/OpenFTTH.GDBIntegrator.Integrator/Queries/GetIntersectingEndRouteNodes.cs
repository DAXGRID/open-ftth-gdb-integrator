using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Queries
{
    public class GetIntersectingEndRouteNodes : IRequest<List<RouteNode>>
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class GetIntersectingEndRouteNodesHandler : IRequestHandler<GetIntersectingEndRouteNodes, List<RouteNode>>
    {
        private readonly IGeoDatabase _geoDatabase;

        public GetIntersectingEndRouteNodesHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<List<RouteNode>> Handle(GetIntersectingEndRouteNodes request, CancellationToken cancellationToken)
        {
            var routeNodes = await _geoDatabase.GetIntersectingEndRouteNodes(request.RouteSegment);
            return routeNodes;
        }
    }
}
