using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteSegmentInfoCommandFactory
    {
        Task<IEnumerable<INotification>> Create(RouteSegment before, RouteSegment after);
    }
}
