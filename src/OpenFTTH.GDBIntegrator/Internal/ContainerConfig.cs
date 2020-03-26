using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka;
using OpenFTTH.GDBIntegrator.Subscriber;

namespace OpenFTTH.GDBIntegrator.Internal
{
    public static class ContainerConfig
    {
        public static IServiceProvider Configure()
        {
            var serviceCollection = SetupServiceCollection();
            var containerBuilder = new ContainerBuilder();

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

        private static void RegisterTypes(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<Startup>().As<IStartup>().OwnedByLifetimeScope();
            containerBuilder.RegisterType<PostgresSubscriber>().As<ISubscriber>().OwnedByLifetimeScope();
        }
    }
}
