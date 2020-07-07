using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Queries
{
    public class GetIntersectingRouteNodes : IRequest<List<RouteNode>>
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class GetIntersectingRouteNodesHandler : IRequestHandler<GetIntersectingRouteNodes, List<RouteNode>>
    {
        public async Task<List<RouteNode>> Handle(GetIntersectingRouteNodes request, CancellationToken cancellationToken)
        {
            return new List<RouteNode>();
        }
    }
}
