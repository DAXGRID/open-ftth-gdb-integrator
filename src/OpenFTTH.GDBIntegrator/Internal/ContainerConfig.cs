using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka;
using Microsoft.Extensions.Configuration;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Hosting;
using OpenFTTH.GDBIntegrator.Subscriber;

namespace OpenFTTH.GDBIntegrator.Internal
{
    public static class ContainerConfig
    {
        public static IHost Configure()
        {
            var hostBuilder = new HostBuilder();

            ConfigureApp(hostBuilder);
            ConfigureLogging(hostBuilder);
            ConfigureServices(hostBuilder);

            return hostBuilder.Build();
        }

        private static void ConfigureApp(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", false);
                config.AddEnvironmentVariables();
            });
        }

        private static void ConfigureServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.Configure<KafkaSetting>(kafkaSettings =>
                                                 hostContext.Configuration.GetSection("kafka").Bind(kafkaSettings));
                services.AddLogging();

                services.AddHostedService<Startup>();
                services.AddScoped<ISubscriber, PostgresSubscriber>();
            });
        }

        private static void ConfigureLogging(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            });
        }
    }
}
