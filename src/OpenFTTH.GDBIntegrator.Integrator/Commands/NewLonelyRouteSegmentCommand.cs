using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewLonelyRouteSegmentCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewLonelyRouteSegmentCommandHandler : AsyncRequestHandler<NewLonelyRouteSegmentCommand>
    {
        private readonly IGeoDatabase _geoDatabase;

        public NewLonelyRouteSegmentCommandHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        protected override async Task Handle(NewLonelyRouteSegmentCommand request, CancellationToken cancellationToken)
        {
            var routeSegment = request.RouteSegment;

            await _geoDatabase.InsertRouteNode(routeSegment.FindStartNode());
            await _geoDatabase.InsertRouteNode(routeSegment.FindEndNode());
        }
    }
}
