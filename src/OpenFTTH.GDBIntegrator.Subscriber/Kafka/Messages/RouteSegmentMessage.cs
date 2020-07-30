using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Messages
{
    public class RouteSegmentMessage
    {
        public RouteSegment Before { get; }
        public RouteSegment After { get; }

        public RouteSegmentMessage() {}

        public RouteSegmentMessage(RouteSegment before, RouteSegment after)
        {
            Before = before;
            After = after;
        }
    }
}
