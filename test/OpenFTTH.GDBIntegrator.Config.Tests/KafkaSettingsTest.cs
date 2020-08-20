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
            var eventRouteNetwork = "event.route-network";
            var postgisRouteNetworkConsumer = "postgis-route-network-consumer";
            var postgisRouteNetworkTopic = "postgis.route-network";


            var kafkaSettings = new KafkaSetting
            {
                Server = server,
                PositionFilePath = positionFilePath,
                EventRouteNetworkTopicName = eventRouteNetwork,
                PostgisRouteNetworkConsumer = postgisRouteNetworkConsumer,
                PostgisRouteNetworkTopic = postgisRouteNetworkTopic
            };

            using (new AssertionScope())
            {
                kafkaSettings.Server.Should().BeEquivalentTo(server);
                kafkaSettings.PositionFilePath.Should().BeEquivalentTo(positionFilePath);
                kafkaSettings.EventRouteNetworkTopicName.Should().BeEquivalentTo(eventRouteNetwork);
                kafkaSettings.PostgisRouteNetworkConsumer.Should().BeEquivalentTo(postgisRouteNetworkConsumer);
                kafkaSettings.PostgisRouteNetworkTopic.Should().BeEquivalentTo(postgisRouteNetworkTopic);
            }
        }
    }
}
