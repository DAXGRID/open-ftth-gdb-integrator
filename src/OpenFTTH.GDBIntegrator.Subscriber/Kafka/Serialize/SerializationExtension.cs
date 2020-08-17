using System;
using Topos.Config;
using Topos.Serialization;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public static class SerializationExtension
    {
        public static void RouteSegment(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer)
                .Register(c => new RouteSegmentSerializer(new SerializationMapper()));
        }

        public static void RouteNode(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer)
                .Register(c => new RouteNodeSerializer(new SerializationMapper()));
        }
    }
}
