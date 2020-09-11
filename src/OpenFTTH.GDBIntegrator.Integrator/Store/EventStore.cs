using OpenFTTH.Events;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public class EventStore : IEventStore
    {
        private List<RouteNetworkCommand> _routeNetworkCommands = new List<RouteNetworkCommand>();

        public void Insert(RouteNetworkCommand routeNetworkCommand)
        {
            _routeNetworkCommands.Add(routeNetworkCommand);
        }

        public IEnumerable<RouteNetworkCommand> Get()
        {
            return _routeNetworkCommands;
        }

        public void Clear()
        {
            _routeNetworkCommands.Clear();
        }
    }
}
