using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentAdded
    {
        public readonly string EventType = "RouteSegmentAddedCommand";
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public Guid EventId { get; }
        public Guid SegmentId { get; }
        public Guid FromNodeId { get; }
        public Guid ToNodeId { get; }
        public string Geometry { get; }

        public RouteSegmentAdded(Guid eventId, Guid segmentId, Guid fromNodeId, Guid toNodeId, string geometry)
        {
            EventId = eventId;
            SegmentId = segmentId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Geometry = geometry;
        }
    }
}
