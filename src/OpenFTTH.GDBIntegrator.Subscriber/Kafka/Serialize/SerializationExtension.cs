using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using System;
using Topos.Config;
using Topos.Serialization;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public static class SerializationExtension
    {
        public static void RouteNetwork(this StandardConfigurer<IMessageSerializer> configurer, IServiceProvider serviceProvider)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            var infoMapper = serviceProvider.GetRequiredService<IInfoMapper>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory!.CreateLogger<RouteNetworkSerializer>();

            StandardConfigurer.Open(configurer)
                .Register(c => new RouteNetworkSerializer(infoMapper, logger));
        }
    }
}
