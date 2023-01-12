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

        public async Task Produce(Guid streamId, object toposMessage)
        {
            var currentVersion = 0L;

            if (_eventStore.Aggregates.CheckIfAggregateIdHasBeenUsed(streamId))
            {
                // We retrieve the latest version each time,
                // so we don't have to keep internal version state up to date.
                currentVersion = await _eventStore
                   .CurrentStreamVersionAsync(streamId) ??
                   throw new InvalidOperationException(
                       "Could not get stream version. The method returned null.");
            }

            // Next expected version is current version plus one.
            var nextExpectedVersion = currentVersion + 1;

            await _eventStore
                .AppendStreamAsync(
                    streamId,
                    nextExpectedVersion,
                    new[] { toposMessage });
        }
    }
}
