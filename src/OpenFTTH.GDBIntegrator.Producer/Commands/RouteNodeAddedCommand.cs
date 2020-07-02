using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Producer.Commands
{
    public class RouteNodeAddedCommand : IRequest
    {
        public RouteNode RouteNode { get; set; }
    }

    public class RouteNodeAddedCommandHandler : IRequestHandler<RouteNodeAddedCommand, Unit>
    {
        public Task<Unit> Handle(RouteNodeAddedCommand request, CancellationToken cancellationToken)
        {
            return default;
        }
    }
}
