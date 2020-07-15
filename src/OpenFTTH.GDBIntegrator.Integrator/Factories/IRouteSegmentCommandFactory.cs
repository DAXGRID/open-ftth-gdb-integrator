using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentCommandFactory
    {
        Task<IRequest> Create(RouteSegment routeSegment);
    }
}
