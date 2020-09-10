using OpenFTTH.Events;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public class EventStore : IEventStore
    {
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        public void Insert(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IEnumerable<IDomainEvent> Get()
        {
            return _domainEvents;
        }

        public void Clear()
        {
            _domainEvents.Clear();
        }
    }
}
