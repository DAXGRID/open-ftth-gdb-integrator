using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentRemoved
    {
        public readonly string EventType = nameof(RouteSegmentRemoved);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public string CmdType { get; }
        public Guid CmdId { get; }
        public Guid SegmentId { get; }
        public IEnumerable<Guid> ReplacedBySegments { get; }
        public bool IsLastEventInCmd { get; }

        public RouteSegmentRemoved
        (
            Guid cmdId,
            Guid segmentId,
            IEnumerable<Guid> replacedBySegments,
            string cmdType,
            bool isLastEventInCmd = false
        )
        {
            if (replacedBySegments.Count() > 2)
                throw new ArgumentOutOfRangeException($"{nameof(ReplacedBySegments)} count cannot be more than 2");

            CmdId = cmdId;
            SegmentId = segmentId;
            ReplacedBySegments = replacedBySegments;
            CmdType = cmdType;
            IsLastEventInCmd = isLastEventInCmd;
        }
    }
}
