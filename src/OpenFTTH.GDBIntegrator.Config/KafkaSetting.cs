namespace OpenFTTH.GDBIntegrator.Config
{
    public class KafkaSetting
    {
        public string Server { get; set; }
        public string Topic { get; set; }
        public string Consumer { get; set; }
        public string PositionFilePath { get; set; }
    }
}
