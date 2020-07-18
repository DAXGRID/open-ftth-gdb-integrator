using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Queries
{
    public class GetIntersectingRouteSegmentsOnRouteNode : IRequest<List<RouteSegment>>
    {
        public RouteNode RouteNode { get; set; }
    }

    public class GetIntersectingRouteSegmentsOnRouteNodeHandler : IRequestHandler<GetIntersectingRouteSegmentsOnRouteNode, List<RouteSegment>>
    {
        private readonly IGeoDatabase _geoDatabase;

        public GetIntersectingRouteSegmentsOnRouteNodeHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<List<RouteSegment>> Handle(GetIntersectingRouteSegmentsOnRouteNode request, CancellationToken cancellationToken)
        {
            var routeSegments = await _geoDatabase.GetIntersectingRouteSegments(request.RouteNode);
            return routeSegments;
        }
    }
}
