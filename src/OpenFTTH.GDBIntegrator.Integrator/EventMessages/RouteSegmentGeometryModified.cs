using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentGeometryModified
    {
        public readonly string EventType = nameof(RouteSegmentGeometryModified);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public Guid CmdId { get; }
        public string CmdType { get; }
        public Guid SegmentId { get; }
        public string Geometry { get; }

        public RouteSegmentGeometryModified(Guid cmdId, Guid segmentId, string cmdType, string geometry)
        {
            CmdId = cmdId;
            SegmentId = segmentId;
            CmdType = cmdType;
            Geometry = geometry;
        }
    }
}
