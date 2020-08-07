using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteSegmentAdded
    {
        public readonly string EventType = nameof(RouteSegmentAdded);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public Guid CmdId { get; }
        public string CmdType { get; }
        public Guid SegmentId { get; }
        public Guid FromNodeId { get; }
        public Guid ToNodeId { get; }
        public string Geometry { get; }
        public string SegmentKind { get; }
        public Guid WorkTaskMrid { get; }
        public string Username { get; }
        public string ApplicationName { get; }
        public string ApplicationInfo { get; }
        public bool IsLastEventInCmd { get; }

        public RouteSegmentAdded(
            Guid cmdId,
            Guid segmentId,
            Guid fromNodeId,
            Guid toNodeId,
            string geometry,
            string cmdType,
            string segmentKind,
            Guid workTaskMrid,
            string username,
            string applicationName,
            string applicationInfo,
            bool isLastEventInCmd = false)
        {
            CmdId = cmdId;
            SegmentId = segmentId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Geometry = geometry;
            CmdType = cmdType;
            SegmentKind = segmentKind;
            WorkTaskMrid = workTaskMrid;
            Username = username;
            ApplicationName = applicationName;
            ApplicationInfo = applicationInfo;
            IsLastEventInCmd = isLastEventInCmd;
        }
    }
}
