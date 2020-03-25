using System;
using Topos.Config;
using Topos.Serialization;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public static class SerializationExtension
    {
        public static void RouteSegment(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer)
                .Register(c => new RouteSegmentSerializer());
        }
    }
}
