using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka;
using OpenFTTH.GDBIntegrator.Subscriber;
using Microsoft.Extensions.Configuration;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Internal
{
    public static class ContainerConfig
    {
        public static IServiceProvider Configure()
        {
            var serviceCollection = SetupServiceCollection();
            var containerBuilder = new ContainerBuilder();

            var appSettingsConfig = BuildConfig();

            serviceCollection
                .AddOptions()
                .Configure<KafkaSetting>(kafkaSettings => appSettingsConfig.GetSection("kafka").Bind(kafkaSettings));

            containerBuilder.Populate(serviceCollection);

            RegisterTypes(containerBuilder);
            var container = containerBuilder.Build();

            return new AutofacServiceProvider(container);
        }

        private static IServiceCollection SetupServiceCollection()
        {
            return new ServiceCollection()
                .AddLogging()
                .Configure<LoggerFilterOptions>(x => x.MinLevel = LogLevel.Trace);
        }

        private static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void RegisterTypes(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<Startup>().OwnedByLifetimeScope();
            containerBuilder.RegisterType<PostgresSubscriber>().As<ISubscriber>().OwnedByLifetimeScope();
        }
    }
}
