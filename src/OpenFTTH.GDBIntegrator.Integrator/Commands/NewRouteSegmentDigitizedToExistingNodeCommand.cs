using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class NewRouteSegmentDigitizedToExistingNodeCommand : IRequest
    {

    }

     public class NewRouteSegmentDigitizedToExistingNodeHandler : AsyncRequestHandler<NewRouteSegmentDigitizedToExistingNodeCommand>
     {
         protected override async Task Handle(NewRouteSegmentDigitizedToExistingNodeCommand request, CancellationToken cancellationToken)
         {

         }
     }
}
