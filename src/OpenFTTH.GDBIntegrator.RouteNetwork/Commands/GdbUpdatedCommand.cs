using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Commands
{
    public class GdbUpdatedCommand : IRequest
    {

    }

    public class GdbUpdatedCommandHandler : IRequestHandler<GdbUpdatedCommand, Unit>
    {
        public Task<Unit> Handle(GdbUpdatedCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
