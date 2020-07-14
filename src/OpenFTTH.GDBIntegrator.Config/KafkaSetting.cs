namespace OpenFTTH.GDBIntegrator.Config
{
    public class KafkaSetting
    {
        public string Server { get; set; }
        public string PostgresRouteSegmentTopic { get; set; }
        public string PostgresRouteSegmentConsumer { get; set; }
        public string PostgresRouteNodeTopic { get; set; }
        public string PostgresRouteNodeConsumer { get; set; }
        public string PositionFilePath { get; set; }
        public string EventRouteNetworkTopicName { get; set; }
    }
}
