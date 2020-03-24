using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace OpenFTTH.GDBIntegrator.Internal
{
    public static class ContainerConfig
    {
        public static IServiceProvider Configure()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .Configure<LoggerFilterOptions>(x => x.MinLevel = LogLevel.Trace);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(serviceCollection);

            RegisterTypes(containerBuilder);

            var container = containerBuilder.Build();

            return new AutofacServiceProvider(container);
        }

        private static void RegisterTypes(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<Startup>().As<IStartup>().OwnedByLifetimeScope();
        }
    }
}
