using System;
using Topos.Config;
using Topos.Producer;
using Topos.Serialization;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Producer.Kafka
{
    public class Producer : IProducer
    {
        private readonly KafkaSetting _kafkaSetting;
        private IToposProducer _producer;

        public Producer(IOptions<KafkaSetting> kafkaSetting)
        {
            _kafkaSetting = kafkaSetting.Value;

            _producer = Configure.Producer(c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();
        }

        public Task Produce(string topicName, ToposMessage toposMessage)
        {
            return default;
        }

        public Task Produce(string topicName, ToposMessage toposMessage, string partitionKey)
        {
            return default;
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
