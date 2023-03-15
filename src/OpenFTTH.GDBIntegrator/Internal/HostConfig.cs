using FluentMigrator.Runner;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenFTTH.EventSourcing.Postgres;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Integrator.Validate;
using OpenFTTH.GDBIntegrator.Integrator.WorkTask;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Producer.EventStore;
using OpenFTTH.GDBIntegrator.Producer.NotificationServer;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Reflection;

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
                settings.TypeNameHandling = TypeNameHandling.Auto;

                return settings;
            });
        }

        private static void ConfigureServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddMediatR(typeof(GeoDatabaseUpdated).GetTypeInfo().Assembly);

                services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                                 .AddPostgres()
                                 .WithGlobalConnectionString(CreatePostgresConnectionString())
                                 .ScanIn(typeof(InitialDatabaseSetup).Assembly).For.Migrations());

                services.AddHostedService<Startup>();
                services.AddSingleton<IRouteNetworkSubscriber, PostgresRouteNetworkSubscriber>();
                services.AddSingleton<IProducer, EventStoreProducer>();
                services.AddSingleton<IGeoDatabase, Postgis>();
                services.AddTransient<IRouteSegmentValidator, RouteSegmentValidator>();
                services.AddTransient<IRouteSegmentFactory, RouteSegmentFactory>();
                services.AddTransient<IRouteNodeFactory, RouteNodeFactory>();
                services.AddTransient<IEnvelopeFactory, EnvelopeFactory>();
                services.AddTransient<IRouteSegmentCommandFactory, RouteSegmentCommandFactory>();
                services.AddTransient<IRouteNodeCommandFactory, RouteNodeCommandFactory>();
                services.AddTransient<IRouteSegmentEventFactory, RouteSegmentEventFactory>();
                services.AddTransient<IRouteNodeEventFactory, RouteNodeEventFactory>();
                services.AddTransient<IInfoMapper, InfoMapper>();
                services.AddSingleton<IRouteNodeValidator, RouteNodeValidator>();

                // This is not the event store with database, this is a local implementation of a place
                // to store events globally before being processed.
                services.AddSingleton<IEventStore, EventStore>();

                services.AddSingleton<IModifiedGeometriesStore, ModifiedGeometriesStore>();
                services.AddTransient<IRouteNodeInfoCommandFactory, RouteNodeInfoCommandFactory>();
                services.AddTransient<IRouteSegmentInfoCommandFactory, RouteSegmentInfoCommandFactory>();
                services.AddTransient<IModifiedEventFactory, ModifiedEventFactory>();
                services.AddTransient<IValidationService, ValidationService>();
                services.AddHttpClient<IValidationService, ValidationService>();
                services.AddTransient<IWorkTaskService, WorkTaskService>();
                services.AddHttpClient<IWorkTaskService, WorkTaskService>();
                services.AddSingleton<INotificationClient, NotificationServerClient>();
                services.AddSingleton<OpenFTTH.EventSourcing.IEventStore>(
                    e =>
                    new PostgresEventStore(
                        serviceProvider: e.GetRequiredService<IServiceProvider>(),
                        connectionString: e.GetRequiredService<IOptions<EventStoreSetting>>().Value.ConnectionString,
                        databaseSchemaName: "events"
                    )
                );

                services.Configure<KafkaSetting>(
                    kafkaSettings => hostContext.Configuration.GetSection("kafka").Bind(kafkaSettings));

                services.Configure<PostgisSetting>(
                    postgisSettings => hostContext.Configuration.GetSection("postgis").Bind(postgisSettings));

                services.Configure<NotificationServerSetting>(
                    notificationServerSetting => hostContext.Configuration.GetSection("notificationServer").Bind(notificationServerSetting));

                services.Configure<ApplicationSetting>(
                    applicationSettings => hostContext.Configuration.GetSection("application").Bind(applicationSettings));

                services.Configure<EventStoreSetting>(
                        eventStoreSetting => hostContext.Configuration.GetSection("eventStore").Bind(eventStoreSetting));
            });
        }

        private static void ConfigureLogging(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var loggingConfiguration = new ConfigurationBuilder()
                   .AddEnvironmentVariables().Build();

                services.AddLogging(loggingBuilder =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(loggingConfiguration)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(new CompactJsonFormatter())
                        .CreateLogger();

                    loggingBuilder.AddSerilog(logger, true);
                });
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
    }
}
