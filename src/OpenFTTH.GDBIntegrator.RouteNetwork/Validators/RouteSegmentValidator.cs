using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Validators
{
    public class RouteSegmentValidator : IRouteSegmentValidator
    {
        private readonly ILogger<RouteSegmentValidator> _logger;
        private readonly ApplicationSetting _applicationSettings;

        public RouteSegmentValidator(ILogger<RouteSegmentValidator> logger, IOptions<ApplicationSetting> applicationSettings)
        {
            _logger = logger;
            _applicationSettings = applicationSettings.Value;
        }

        public bool LineIsValid(LineString lineString)
        {
            if (!lineString.IsValid)
            {
                LogValidationError("IsValid", lineString);
                return false;
            }

            // We don't want lines that are not simple - i.e. self intersecting
            if (!lineString.IsSimple)
            {
                LogValidationError("IsSimple", lineString);
                return false;
            }

            // We don't want lines that are closes - i.e. where the ends of the line is snapped together
            if (lineString.IsClosed)
            {
                LogValidationError("IsClosed", lineString);
                return false;
            }

            // We don't want ends closer to each other than tolerance
            if (lineString.StartPoint.Distance(lineString.EndPoint) < _applicationSettings.Tolerance)
            {
                LogValidationError("EndsCloserToEachOtherThanTolereance", lineString);
                return false;
            }

            // We don't want ends closer to the edge than tolerance
            if (!GeometrySnapper.SnapToSelf(lineString, _applicationSettings.Tolerance, false).Equals(lineString))
            {
                LogValidationError("EndsCloserToTheEdgeThanTolereance", lineString);
                return false;
            }

            return true;
        }

        private void LogValidationError(string errorName, LineString lineString)
        {
            _logger.LogError($"Validation failed on '{errorName}'. WkbString: '{lineString.ToString()}'");
        }
    }
}
