namespace OpenFTTH.GDBIntegrator.Config
{
    public class KafkaSetting
    {
        public string Server { get; set; }
        public string PostgisRouteNetworkTopic { get; set; }
        public string PostgisRouteNetworkConsumer { get; set; }
        public string PositionFilePath { get; set; }
        public string EventRouteNetworkTopicName { get; set; }
        public string EventGeographicalAreaUpdated { get; set; }
    }
}
