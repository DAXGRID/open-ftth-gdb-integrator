using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentMarkedForDeletion
    {
        public readonly string EventType = nameof(RouteSegmentMarkedForDeletion);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public Guid CmdId { get; }
        public string CmdType { get; }
        public Guid SegmentId { get; }

        public RouteSegmentMarkedForDeletion(Guid cmdId, Guid segmentId, string cmdType)
        {
            CmdId = cmdId;
            SegmentId = segmentId;
            CmdType = cmdType;
        }
    }
}
