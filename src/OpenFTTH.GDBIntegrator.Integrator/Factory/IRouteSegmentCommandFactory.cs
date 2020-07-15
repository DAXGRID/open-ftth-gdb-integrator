using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factory
{
    public interface IRouteSegmentCommandFactory
    {
        Task<IRequest> Create(RouteSegment routeSegment);
    }
}
