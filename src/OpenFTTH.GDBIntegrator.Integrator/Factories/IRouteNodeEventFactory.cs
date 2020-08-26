using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeEventFactory
    {
        Task<List<INotification>> CreateDigitizedEvent(RouteNode routeNode);
        Task<INotification> CreateUpdatedEvent(RouteNode before, RouteNode after);
    }
}
