using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using MediatR;
using FluentMigrator.Runner;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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
            ConfigureJsonConverter();

            return hostBuilder.Build();
        }

        private static void ConfigureApp(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            });
        }

        private static void ConfigureJsonConverter()
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                settings.Converters.Add(new StringEnumConverter());

                return settings;
            });
        }

        private static void ConfigureServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddLogging();
                services.AddMediatR(typeof(GeoDatabaseUpdated).GetTypeInfo().Assembly);

                services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                                 .AddPostgres()
                                 .WithGlobalConnectionString(CreatePostgresConnectionString())
                                 .ScanIn(typeof(InitialDatabaseSetup).Assembly).For.Migrations());

                services.AddHostedService<Startup>();
                services.AddSingleton<IRouteNetworkSubscriber, PostgresRouteNetworkSubscriber>();
                services.AddSingleton<IProducer, Producer.Kafka.Producer>();
                services.AddTransient<IGeoDatabase, Postgis>();
                services.AddTransient<IRouteSegmentValidator, RouteSegmentValidator>();
                services.AddTransient<IRouteSegmentFactory, RouteSegmentFactory>();
                services.AddTransient<IRouteNodeFactory, RouteNodeFactory>();
                services.AddTransient<IRouteSegmentEventFactory, RouteSegmentEventFactory>();
                services.AddTransient<IRouteNodeEventFactory, RouteNodeEventFactory>();
                services.AddTransient<IInfoMapper, InfoMapper>();

                services.Configure<KafkaSetting>(kafkaSettings =>
                                                 hostContext.Configuration.GetSection("kafka").Bind(kafkaSettings));

                services.Configure<PostgisSetting>(postgisSettings =>
                                                 hostContext.Configuration.GetSection("postgis").Bind(postgisSettings));

                services.Configure<ApplicationSetting>(applicationSettings =>
                                                 hostContext.Configuration.GetSection("application").Bind(applicationSettings));
            });
        }

        private static string CreatePostgresConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("POSTGIS__HOST");
            var port = Environment.GetEnvironmentVariable("POSTGIS__PORT");
            var username = Environment.GetEnvironmentVariable("POSTGIS__USERNAME");
            var password = Environment.GetEnvironmentVariable("POSTGIS__PASSWORD");
            var database = Environment.GetEnvironmentVariable("POSTGIS__DATABASE");

            var connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";
            return connectionString;
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
