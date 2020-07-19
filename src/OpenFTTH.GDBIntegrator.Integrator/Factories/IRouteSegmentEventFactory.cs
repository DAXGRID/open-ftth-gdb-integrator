using MediatR;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentEventFactory
    {
        Task<object> Create(RouteSegment routeSegment);
    }
}
