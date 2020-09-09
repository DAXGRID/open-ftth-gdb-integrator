using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Confluent.Kafka;

namespace OpenFTTH.GDBIntegrator.Producer.Kafka
{
    public class Producer : IProducer
    {
        private readonly KafkaSetting _kafkaSetting;
        private Confluent.Kafka.IProducer<Null, string> _producer;
        private readonly ILogger<Producer> _logger;

        public Producer(IOptions<KafkaSetting> kafkaSetting, ILogger<Producer> logger)
        {
            _kafkaSetting = kafkaSetting.Value;
            _logger = logger;
        }

        public void Init()
        {

        }

        public async Task Produce(string topicName, object message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSetting.Server,
                EnableIdempotence = true,
                LingerMs = 0.5,
                TransactionalId = "Test123"
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    _logger.LogDebug($"Sending message topicname: {topicName} and body {JsonConvert.SerializeObject(message, Formatting.Indented)}");

                    producer.InitTransactions(TimeSpan.FromSeconds(30));
                    producer.BeginTransaction();

                    var deliveryResult = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(message) });

                    producer.CommitTransaction(TimeSpan.FromSeconds(30));
                    producer.Flush();
                }
                catch (ProduceException<Null, string> e)
                {
                    _logger.LogError($"Delivery failed : {e.Error.Reason}");
                    producer.AbortTransaction(TimeSpan.FromSeconds(30));
                    throw;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
