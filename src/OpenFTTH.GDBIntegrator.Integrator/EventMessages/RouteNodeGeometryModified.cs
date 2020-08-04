using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeGeometryModified
    {
        public readonly string EventType = nameof(RouteSegmentGeometryModified);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public Guid CmdId { get; }
        public string CmdType { get; }
        public Guid NodeId { get; }
        public string Geometry { get; }

        public RouteNodeGeometryModified(Guid cmdId, Guid nodeId, string cmdType, string geometry)
        {
            CmdId = cmdId;
            NodeId = nodeId;
            CmdType = cmdType;
            Geometry = geometry;
        }
    }
}
