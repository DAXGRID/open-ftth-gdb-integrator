using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using Topos.Serialization;
using Topos.Consumer;
using System.Collections.Generic;
using FluentAssertions;
using System;
using System.Text;

namespace OpenFTTH.GDBIntegrator.Subscriber.Tests.Kafka.Serialize
{
    public class RouteNodeSerializerTest
    {
        [Fact]
        public void Deserialize_ShouldThrowArgumentNullException_OnReceivedLogicalMessageBeingNull()
        {
            var routeSegmentSerializer = new RouteNodeSerializer();

            ReceivedTransportMessage receivedTransportMessage = null;

            routeSegmentSerializer.Invoking(x => x.Deserialize(receivedTransportMessage))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyNull()
        {
            var routeSegmentSerializer = new RouteNodeSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            byte[] body = null;

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);
            var expected = new ReceivedLogicalMessage(headers, new RouteNodeMessage(), position);

            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyLengthIsZero()
        {
            var routeNodeSerializer = new RouteNodeSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = new byte[0];

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, null);
            var expected = new ReceivedLogicalMessage(headers, new RouteNodeMessage(), position);

            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteNodeSerializerMessageBeforeIsNull.json")]
        public void Deserialize_ShouldReturnDeserializedMessageWithBeforebeingNull_OnValidReceivedTransportMessageWithBeforeBeingNull(string fileData)
        {
            var routeNodeSerializer = new RouteNodeSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            RouteNode expectedRouteNodeBefore = null;

            var expectedRouteNodeAfter = new RouteNode
            (
                new Guid("de39df61-8e2b-4132-a7c2-c55c77b98578"),
                Convert.FromBase64String("AQEAACDoZAAA4tDwso11IEGdihg1rXlXQQ=="),
                Guid.Empty,
                string.Empty,
                string.Empty,
                false
            );

            var expectedBody = new RouteNodeMessage(expectedRouteNodeBefore, expectedRouteNodeAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedBody, position);
            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteNodeSerializerMessage.json")]
        public void Deserialize_ShouldReturnDeserializedMessage_OnValidReceivedTransportMessage(string fileData)
        {
            var routeNodeSerializer = new RouteNodeSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            var expectedRouteNodeBefore = new RouteNode
            (
                new Guid("de39df61-8e2b-4132-a7c2-c55c77b98578"),
                Convert.FromBase64String("AQEAACDoZAAA4tDwso11IEGdihg1rXlXQQ=="),
                Guid.Empty,
                string.Empty,
                string.Empty,
                false
            );

            var expectedRouteNodeAfter = new RouteNode
            (
                new Guid("de39df61-8e2b-4132-a7c2-c55c77b98578"),
                Convert.FromBase64String("AQEAACDoZAAA4tDwso11IEGdihg1rXlXQQ=="),
                Guid.Empty,
                string.Empty,
                string.Empty,
                true
            );

            var expectedBody = new RouteNodeMessage(expectedRouteNodeBefore, expectedRouteNodeAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedBody, position);
            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
