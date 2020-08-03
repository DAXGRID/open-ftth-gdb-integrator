using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public interface IRouteSegmentFactory
    {
        /// <summary>
        ///  Takes in WKT containing GeoCollection
        /// </summary>
        List<RouteSegment> Create(string text);
        /// <summary>
        ///  Takes in LineString and returns RouteSegment
        /// </summary>
        RouteSegment Create(LineString lineString);
    }
}
