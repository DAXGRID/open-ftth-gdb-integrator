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
            var postgresRouteSegmentTopic = "event.route-network_route_segment";
            var postgresRouteSegmentConsumer = "postgis-consumer_route_segment";
            var eventRouteNetwork = "event.route-network";
            var postgresRouteNodeTopic = "event.route-network_route_node";
            var postgresRouteNodeConsumer = "postgis-consumer_route_node";

            var kafkaSettings = new KafkaSetting
            {
                Server = server,
                PositionFilePath = positionFilePath,
                PostgresRouteSegmentTopic = postgresRouteSegmentTopic,
                PostgresRouteSegmentConsumer = postgresRouteSegmentConsumer,
                EventRouteNetworkTopicName = eventRouteNetwork,
                PostgresRouteNodeTopic = postgresRouteNodeTopic,
                PostgresRouteNodeConsumer = postgresRouteNodeConsumer
            };

            using (new AssertionScope())
            {
                kafkaSettings.Server.Should().BeEquivalentTo(server);
                kafkaSettings.PositionFilePath.Should().BeEquivalentTo(positionFilePath);
                kafkaSettings.PostgresRouteSegmentTopic.Should().BeEquivalentTo(postgresRouteSegmentTopic);
                kafkaSettings.PostgresRouteSegmentConsumer.Should().BeEquivalentTo(postgresRouteSegmentConsumer);
                kafkaSettings.EventRouteNetworkTopicName.Should().BeEquivalentTo(eventRouteNetwork);
                kafkaSettings.PostgresRouteNodeTopic.Should().BeEquivalentTo(postgresRouteNodeTopic);
                kafkaSettings.PostgresRouteNodeConsumer.Should().BeEquivalentTo(postgresRouteNodeConsumer);
            }
        }
    }
}
