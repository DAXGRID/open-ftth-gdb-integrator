using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentBetweenTwoExistingNodesCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewRouteSegmentBetweenTwoExistingNodesCommandHandler : AsyncRequestHandler<NewRouteSegmentBetweenTwoExistingNodesCommand>
    {
        private readonly IGeoDatabase _geoDatabase;

        public NewRouteSegmentBetweenTwoExistingNodesCommandHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        protected override async Task Handle(NewRouteSegmentBetweenTwoExistingNodesCommand request, CancellationToken cancellationToken)
        {
            var routeSegment = request.RouteSegment;
        }
    }
}
