using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Validators
{
    public interface IRouteSegmentValidator
    {
        /// <summary>
        /// Logic that checks if a line drawn by user (representing a route segment) is ok to be futher processed
        /// </summary>
        /// <returns>bool</returns>
        bool LineIsValid(LineString lineString);
    }
}
