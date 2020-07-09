using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewConnectedRouteSegmentCommand : IRequest
    {
        public RouteSegment RouteSegment { get; set; }
    }

    public class NewConnectedRouteSegmentCommandHandler : AsyncRequestHandler<NewConnectedRouteSegmentCommand>
    {
        private readonly IGeoDatabase _geoDatabase;

        public NewConnectedRouteSegmentCommandHandler(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        protected override async Task Handle(NewConnectedRouteSegmentCommand request, CancellationToken cancellationToken)
        {
            var routeSegment = request.RouteSegment;
        }
    }
}
