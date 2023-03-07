using OpenFTTH.GDBIntegrator.RouteNetwork;
using System;

namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public record RouteSegmentMessage
    {
        public Guid EventId { get; init; }
        public RouteSegment Before { get; init; }
        public RouteSegment After { get; init; }

        public RouteSegmentMessage(Guid eventId, RouteSegment before, RouteSegment after)
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
