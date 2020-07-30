using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeMarkedForDeletion
    {
        public readonly string EventType = nameof(RouteNodeMarkedForDeletion);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid CmdId = Guid.NewGuid();
        public Guid EventId { get; }
        public string CmdType { get; }
        public Guid NodeId { get; }

        public RouteNodeMarkedForDeletion(Guid eventId, Guid segmentId, string cmdType)
        {
            EventId = eventId;
            NodeId = segmentId;
            CmdType = cmdType;
        }
    }
}
