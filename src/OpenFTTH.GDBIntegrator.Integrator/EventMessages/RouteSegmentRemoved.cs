using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentRemoved
    {
        public readonly string EventType = "RouteSegmentRemoved";
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public Guid EventId { get; }
        public Guid SegmentId { get; }
        public IEnumerable<Guid> ReplacedBySegments { get; }

        public RouteSegmentRemoved(Guid eventId, Guid segmentId, IEnumerable<Guid> replacedBySegments)
        {
            if (replacedBySegments.Count() > 2)
                throw new ArgumentOutOfRangeException($"{nameof(ReplacedBySegments)} count cannot be more than 2");

            EventId = eventId;
            SegmentId = segmentId;
            ReplacedBySegments = replacedBySegments;
        }
    }
}