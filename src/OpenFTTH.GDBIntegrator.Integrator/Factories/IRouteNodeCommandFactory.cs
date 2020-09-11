using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeCommandFactory
    {
        Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode);
        Task<List<INotification>> CreateUpdatedEvent(RouteNode before, RouteNode after);
    }
}
