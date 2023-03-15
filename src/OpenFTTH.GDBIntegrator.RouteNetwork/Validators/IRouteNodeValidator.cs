using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Validators;

public interface IRouteNodeValidator
{
    /// <summary>
    /// Logic that checks if a point drawn by user (representing a route node) is ok to be futher processed
    /// </summary>
    /// <returns>bool</returns>
    bool PointIsValid(Point point);
}
