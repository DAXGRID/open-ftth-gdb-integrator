using OpenFTTH.Events;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public interface IEventStore
    {
        void Insert(RouteNetworkCommand routeNetworkCommand);
        void Clear();
        IEnumerable<RouteNetworkCommand> Get();
    }
}
