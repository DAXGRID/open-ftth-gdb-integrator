using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeAdded
    {
        public readonly string EventType = "RouteNodeAddedCommand";
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public Guid EventId { get; }
        public Guid NodeId { get; }
        public string Geometry { get; }

        public RouteNodeAdded(Guid eventId, Guid nodeId, string geometry)
        {
            EventId = eventId;
            NodeId = nodeId;
            Geometry = geometry;
        }
    }
}
