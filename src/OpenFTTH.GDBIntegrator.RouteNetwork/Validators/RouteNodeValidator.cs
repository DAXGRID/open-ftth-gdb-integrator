using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Validators;

public class RouteNodeValidator : IRouteNodeValidator
{
    private readonly ILogger<RouteNodeValidator> _logger;

    public RouteNodeValidator(ILogger<RouteNodeValidator> logger)
    {
        _logger = logger;
    }

    public bool PointIsValid(Point point)
    {
        if (!point.IsValid)
        {
            LogValidationError("Point is not valid.", point);
            return false;
        }

        if (point.X == double.NaN || point.Y == double.NaN)
        {
            LogValidationError("Point is not valid.", point);
            return false;
        }

        return true;
    }

    private void LogValidationError(string errorName, Point point)
    {
        _logger.LogError($"Validation failed on '{errorName}'. WkbString: '{point.ToString()}'.");
    }
}
