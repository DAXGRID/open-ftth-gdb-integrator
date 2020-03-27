using Topos.Config;
using OpenFTTH.GDBIntegrator.Model;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using System;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka
{
    public class PostgresSubscriber : ISubscriber
    {
        private IDisposable _consumer;
        private KafkaSetting _kafkaSetting;

        public PostgresSubscriber(IOptions<KafkaSetting> kafkaSetting)
        {
            _kafkaSetting = kafkaSetting.Value;
        }

        public void Subscribe()
        {
            _consumer = Configure
                .Consumer(_kafkaSetting.Consumer, c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.RouteSegment())
                .Topics(t => t.Subscribe(_kafkaSetting.Topic))
                .Positions(p => p.StoreInMemory())
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        var routeSegment = (RouteSegment)message.Body;

                        if (!String.IsNullOrEmpty(routeSegment.Mrid.ToString()))
                            Console.WriteLine(routeSegment.Mrid);
                    }
                }).Start();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
