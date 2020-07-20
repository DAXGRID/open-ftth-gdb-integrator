using MediatR;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentEventFactory
    {
        Task<IEnumerable<INotification>> Create(RouteSegment routeSegment);
    }
}
