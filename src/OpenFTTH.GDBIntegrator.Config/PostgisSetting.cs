namespace OpenFTTH.GDBIntegrator.Config
{
    public class PostgisSetting
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ConnectionString => CreateConnectionString();

        private string CreateConnectionString()
        {
            return $"Host={Host};Port={Port};Username={Username};Password={Password};Database={Database}";
        }
    }
}
