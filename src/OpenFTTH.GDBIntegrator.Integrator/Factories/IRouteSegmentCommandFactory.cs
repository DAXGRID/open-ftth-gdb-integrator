using MediatR;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentCommandFactory
    {
        Task<IEnumerable<INotification>> CreateDigitizedEvent(RouteSegment routeSegment);
        Task<IEnumerable<INotification>> CreateUpdatedEvent(RouteSegment before, RouteSegment after);
    }
}
