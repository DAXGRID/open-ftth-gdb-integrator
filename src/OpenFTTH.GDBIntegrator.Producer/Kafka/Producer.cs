using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Topos.Config;
using Topos.Producer;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Producer.Kafka
{
    public class Producer : IProducer
    {
        private readonly KafkaSetting _kafkaSetting;
        private IToposProducer _producer;

        public Producer(IOptions<KafkaSetting> kafkaSetting)
        {
            _kafkaSetting = kafkaSetting.Value;
        }

        public void Init()
        {
            if (_producer is null)
            {
                _producer = Configure.Producer(c => c.UseKafka(_kafkaSetting.Server))
                    .Serialization(s => s.UseNewtonsoftJson())
                    .Create();
            }
        }

        public async Task Produce(string topicName, object toposMessage)
        {
            await _producer.Send(topicName, new ToposMessage(toposMessage));
        }

        public async Task Produce(string topicName, object toposMessage, string partitionKey)
        {
            await _producer.Send(topicName, new ToposMessage(toposMessage), partitionKey);
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
