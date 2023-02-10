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
            var postgisRouteNetworkConsumer = "postgis-route-network-consumer";
            var postgisRouteNetworkTopic = "postgis.route-network";

            var kafkaSettings = new KafkaSetting
            {
                Server = server,
                PostgisRouteNetworkConsumer = postgisRouteNetworkConsumer,
                PostgisRouteNetworkTopic = postgisRouteNetworkTopic
            };

            using (new AssertionScope())
            {
                kafkaSettings.Server.Should().BeEquivalentTo(server);
                kafkaSettings.PostgisRouteNetworkConsumer.Should().BeEquivalentTo(postgisRouteNetworkConsumer);
                kafkaSettings.PostgisRouteNetworkTopic.Should().BeEquivalentTo(postgisRouteNetworkTopic);
            }
        }
    }
}
