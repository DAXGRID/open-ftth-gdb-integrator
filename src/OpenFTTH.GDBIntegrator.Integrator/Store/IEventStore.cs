using OpenFTTH.Events;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public interface IEventStore
    {
        void Insert(IDomainEvent domainEvent);
        void Clear();
        IEnumerable<IDomainEvent> Get();
    }
}
