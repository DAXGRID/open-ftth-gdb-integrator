namespace OpenFTTH.GDBIntegrator.Config
{
    public class ApplicationSetting
    {
        public string ApplicationName { get; set; }
        public double Tolerance { get; set; }
        public bool SendGeographicalAreaUpdatedNotification { get; set; }
        public bool EnableSegmentEndsAutoSnappingToRouteNode { get; set; }
        public string ApiGatewayHost { get; set; }
    }
}
