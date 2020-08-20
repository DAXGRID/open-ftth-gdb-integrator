using System;
using Topos.Config;
using Topos.Serialization;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public static class SerializationExtension
    {
        public static void RouteNetwork(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer)
                .Register(c => new RouteNetworkSerializer(new SerializationMapper()));
        }
    }
}
