using Topos.Config;
using OpenFTTH.GDBIntegrator.Model;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using System;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka
{
    public class PostgresSubscriber : ISubscriber
    {
        private IDisposable _consumer;

        public void Subscribe()
        {
            _consumer = Configure
                .Consumer("postgis-consumer", c => c.UseKafka(""))
                .Serialization(s => s.RouteSegment())
                .Topics(t => t.Subscribe(""))
                .Positions(p => p.StoreInMemory())
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        Console.WriteLine("Received message");
                        var routeSegment = (RouteSegment)message.Body;
                        Console.WriteLine(routeSegment.Mrid);

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
