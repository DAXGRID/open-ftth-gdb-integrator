using OpenFTTH.GDBIntegrator.RouteNetwork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class InvalidRouteNodeOperationCommand : IRequest
    {
        public RouteNode RouteNode { get; set; }
    }

    public class InvalidRouteNodeOperationCommandHandler : IRequestHandler<InvalidRouteNodeOperationCommand, Unit>
    {
        public async Task<Unit> Handle(InvalidRouteNodeOperationCommand command, CancellationToken token)
        {
            return await Task.FromResult(new Unit());
        }
    }
}
