using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public interface IEnvelopeFactory
    {
        public Envelope Create(List<RouteNode> routeNodes, List<RouteSegment> routeSegments);
    }
}
