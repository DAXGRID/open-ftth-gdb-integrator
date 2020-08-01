using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeMarkedForDeletion
    {
        public readonly string EventType = nameof(RouteNodeMarkedForDeletion);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public Guid CmdId { get; }
        public string CmdType { get; }
        public Guid NodeId { get; }

        public RouteNodeMarkedForDeletion(Guid cmdId, Guid segmentId, string cmdType)
        {
            CmdId = cmdId;
            NodeId = segmentId;
            CmdType = cmdType;
        }
    }
}
