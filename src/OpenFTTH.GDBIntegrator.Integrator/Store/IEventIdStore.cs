using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Store;

public interface IEventIdStore
{
    Task<long> LoadEventIds(CancellationToken token = default);
    HashSet<Guid> GetEventIds();
    void AppendEventId(Guid eventId);
}
