using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public class EnvelopeFactory : IEnvelopeFactory
    {
        public Envelope Create(List<RouteNode> routeNodes, List<RouteSegment> routeSegments)
        {
            var envelope = new Envelope();

            foreach (var routeNode in routeNodes)
            {
                envelope.ExpandToInclude(routeNode.GetPoint().EnvelopeInternal);
            }

            foreach (var routeSegment in routeSegments)
            {
                envelope.ExpandToInclude(routeSegment.GetLineString().EnvelopeInternal);
            }

            return envelope;
        }
    }
}
