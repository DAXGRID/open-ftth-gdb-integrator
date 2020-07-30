using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
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
