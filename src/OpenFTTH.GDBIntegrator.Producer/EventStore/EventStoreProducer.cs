using OpenFTTH.EventSourcing;
using System;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Producer.EventStore
{
    public class EventStoreProducer : IProducer
    {
        private readonly IEventStore _eventStore;

        public EventStoreProducer(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task Produce(Guid streamId, object message)
        {
            // We retrieve the latest version each time,
            // so we don't have to keep internal version state up to date.
            var currentVersion = await _eventStore
                .CurrentStreamVersionAsync(streamId) ?? 0L;

            // Next expected version is current version plus one.
            var nextExpectedVersion = currentVersion + 1;

            await _eventStore
                .AppendStreamAsync(
                    streamId,
                    nextExpectedVersion,
                    new[] { message });
        }
    }
}
