using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using MediatR;

namespace OpenFTTH.GDBIntegrator.Internal
{
    public static class HostConfig
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
                config.AddEnvironmentVariables();
                config.AddJsonFile("appsettings.json", false, true);
            });
        }

        private static void ConfigureServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddLogging();
                services.AddMediatR(typeof(GeoDatabaseUpdated).GetTypeInfo().Assembly);

                services.AddHostedService<Startup>();
                services.AddSingleton<IRouteSegmentSubscriber, PostgresRouteSegmentSubscriber>();
                services.AddSingleton<IRouteNodeSubscriber, PostgresRouteNodeSubscriber>();
                services.AddSingleton<IProducer, Producer.Kafka.Producer>();
                services.AddTransient<IGeoDatabase, Postgis>();
                services.AddTransient<IRouteSegmentValidator, RouteSegmentValidator>();
                services.AddTransient<IRouteSegmentFactory, RouteSegmentFactory>();
                services.AddTransient<IRouteSegmentFactory, RouteSegmentFactory>();
                services.AddTransient<IRouteSegmentEventFactory, RouteSegmentEventFactory>();
                services.AddTransient<IRouteNodeEventFactory, RouteNodeEventFactory>();

                services.Configure<KafkaSetting>(kafkaSettings =>
                                                 hostContext.Configuration.GetSection("kafka").Bind(kafkaSettings));

                services.Configure<PostgisSetting>(postgisSettings =>
                                                 hostContext.Configuration.GetSection("postgis").Bind(postgisSettings));

                services.Configure<ApplicationSetting>(applicationSettings =>
                                                 hostContext.Configuration.GetSection("application").Bind(applicationSettings));
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
