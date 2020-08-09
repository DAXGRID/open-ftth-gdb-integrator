using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Topos.Config;
using Topos.Producer;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Producer.Kafka
{
    public class Producer : IProducer
    {
        private readonly KafkaSetting _kafkaSetting;
        private IToposProducer _producer;
        private readonly ILogger<Producer> _logger;

        public Producer(IOptions<KafkaSetting> kafkaSetting, ILogger<Producer> logger)
        {
            _kafkaSetting = kafkaSetting.Value;
            _logger = logger;
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
            _logger.LogInformation($"Sending message topicname: {topicName} and body {JsonConvert.SerializeObject(toposMessage, Formatting.Indented)}");
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
