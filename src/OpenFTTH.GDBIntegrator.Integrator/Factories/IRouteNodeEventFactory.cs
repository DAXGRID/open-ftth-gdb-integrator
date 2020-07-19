using MediatR;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeEventFactory
    {
        Task<object> Create(RouteNode routeNode);
    }
}
