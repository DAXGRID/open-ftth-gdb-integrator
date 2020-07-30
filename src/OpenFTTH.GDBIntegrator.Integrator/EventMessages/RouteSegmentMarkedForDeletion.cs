using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentMarkedForDeletion
    {
        public readonly string EventType = nameof(RouteSegmentMarkedForDeletion);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public Guid EventId { get; }
        public string CmdType { get; }
        public Guid SegmentId { get; }

        public RouteSegmentMarkedForDeletion(Guid eventId, Guid segmentId, string cmdType)
        {
            EventId = eventId;
            SegmentId = segmentId;
            CmdType = cmdType;
        }
    }
}
