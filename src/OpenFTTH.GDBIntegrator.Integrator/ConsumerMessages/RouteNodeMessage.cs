using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;

namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public record RouteNodeMessage
    {
        public Guid EventId { get; init; }
        public RouteNode Before { get; init; }
        public RouteNode After { get; init; }

        public RouteNodeMessage() {}
        public RouteNodeMessage(Guid eventId, RouteNode before, RouteNode after)
        {
            if (eventId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Cannot be default guid.",
                    nameof(eventId));
            }

            EventId = eventId;
            Before = before;
            After = after;
        }
    }
}
