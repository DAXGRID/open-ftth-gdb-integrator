using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeAdded
    {
        public readonly string EventType = nameof(RouteNodeAdded);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public string CmdType { get; }
        public Guid EventId { get; }
        public Guid NodeId { get; }
        public string Geometry { get; }

        public RouteNodeAdded(Guid eventId, Guid nodeId, string geometry, string cmdType)
        {
            EventId = eventId;
            NodeId = nodeId;
            Geometry = geometry;
            CmdType = cmdType;
        }
    }
}
