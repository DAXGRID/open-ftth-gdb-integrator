using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeInfoCommandFactory
    {
        Task<IEnumerable<INotification>> Create(RouteNode before, RouteNode after);
    }
}
