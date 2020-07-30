using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public class RouteNodeMessage
    {
        public RouteNode Before { get; }
        public RouteNode After { get; }

        public RouteNodeMessage() {}

        public RouteNodeMessage(RouteNode before, RouteNode after)
        {
            Before = before;
            After = after;
        }
    }
}
