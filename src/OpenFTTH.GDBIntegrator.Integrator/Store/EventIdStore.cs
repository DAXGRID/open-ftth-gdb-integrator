using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Integrator.Store
{
    public class EventIdStore : IEventIdStore
    {
        private HashSet<Guid> _eventIds;
        private readonly EventStoreSetting _eventStoreSetting;

        public EventIdStore(IOptions<EventStoreSetting> eventStoreSetting)
        {
            _eventStoreSetting = eventStoreSetting.Value;
        }

        public void AppendEventId(Guid eventId)
        {
            _eventIds.Add(eventId);
        }

        public HashSet<Guid> GetEventIds()
        {
            return _eventIds ?? throw new InvalidOperationException("EventIds has not been loaded yet.");
        }

        public async Task<long> LoadEventIds(CancellationToken token = default)
        {
            _eventIds = new();

            await foreach (var eId in GetAllEventIds(token).ConfigureAwait(false))
            {
                _eventIds.Add(eId);
            }

            return _eventIds.Count;
        }

        private async IAsyncEnumerable<Guid> GetAllEventIds(
            [EnumeratorCancellation] CancellationToken token = default)
        {
            const string SQL = @"SELECT data->'EventId' AS event_id
FROM events.mt_events
WHERE type = 'route_network_edit_operation_occured_event'";

            using var conn = new NpgsqlConnection(_eventStoreSetting.ConnectionString);
            using var cmd = new NpgsqlCommand(SQL, conn);

            await conn.OpenAsync(token).ConfigureAwait(false);
            var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);

            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                yield return reader.GetGuid(reader.GetOrdinal("event_id"));
            }
        }
    }
}
