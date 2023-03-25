using System;

namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public record InvalidMessage
    {
        public Guid EventId { get; init; }
        public object Message { get; init; }
        public bool Delete { get; init; }

        public InvalidMessage(object message, Guid eventId)
        {
            Message = message;
            EventId = eventId;
        }

        public InvalidMessage(object message, Guid eventId, bool delete)
        {
            Message = message;
            EventId = eventId;
            Delete = delete;
        }
    }
}
