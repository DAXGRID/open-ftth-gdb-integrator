using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public interface IRouteSegmentFactory
    {
        /// <summary>
        ///  Takes in WKT containing GeoCollection
        /// </summary>
        List<RouteSegment> Create(string text);
    }
}
