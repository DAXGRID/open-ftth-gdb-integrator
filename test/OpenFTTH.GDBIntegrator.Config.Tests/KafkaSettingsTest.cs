using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

namespace OpenFTTH.GDBIntegrator.Config.Tests
{
    public class KafkaSettingsTest
    {
        [Fact]
        public void KafkaSettings_ShouldInitalizeValues_OnConstruction()
        {
            var server = "192.13.2.1";
            var positionFilePath = "/tmp/";
            var topic = "event.route-network";
            var consumer = "postgis-consumer";
            var eventRouteNetwork = "event.route-network";

            var kafkaSettings = new KafkaSetting
            {
                Server = server,
                PositionFilePath = positionFilePath,
                PostgresRouteSegmentTopic = topic,
                PostgresRouteSegmentConsumer = consumer,
                EventRouteNetworkTopicName = eventRouteNetwork
            };

            using (new AssertionScope())
            {
                kafkaSettings.Server.Should().BeEquivalentTo(server);
                kafkaSettings.PositionFilePath.Should().BeEquivalentTo(positionFilePath);
                kafkaSettings.PostgresRouteSegmentTopic.Should().BeEquivalentTo(topic);
                kafkaSettings.PostgresRouteSegmentConsumer.Should().BeEquivalentTo(consumer);
                kafkaSettings.EventRouteNetworkTopicName.Should().BeEquivalentTo(eventRouteNetwork);
            }
        }
    }
}
