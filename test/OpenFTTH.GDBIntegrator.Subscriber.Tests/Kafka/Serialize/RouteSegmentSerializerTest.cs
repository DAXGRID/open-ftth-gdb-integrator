using Xunit;
using OpenFTTH.GDBIntegrator.Model;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using Topos.Serialization;
using Topos.Consumer;
using System.Collections.Generic;
using FluentAssertions;
using System;
using System.Text;

namespace OpenFTTH.GDBIntegrator.Subscriber.Tests.Kafka.Serialize
{
    public class RouteSegmentSerializerTest
    {
        [Fact]
        public void Deserialize_ShouldThrowArgumentNullException_OnReceivedLogicalMessageBeingNull()
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            ReceivedTransportMessage receivedTransportMessage = null;

            routeSegmentSerializer.Invoking(x => x.Deserialize(receivedTransportMessage))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyNull()
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            byte[] body = null;

            var logicalMessage = new ReceivedTransportMessage(position, headers, body);
            var expected = new ReceivedLogicalMessage(headers, new RouteSegment(), position);

            var result = routeSegmentSerializer.Deserialize(logicalMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyLengthIsZero()
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = new byte[0];

            var logicalMessage = new ReceivedTransportMessage(position, headers, null);
            var expected = new ReceivedLogicalMessage(headers, new RouteSegment(), position);

            var result = routeSegmentSerializer.Deserialize(logicalMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteSegmentSerializerMessage.json")]
        public void Deserialize_ShouldReturnDeserializedMessage_OnValidReceivedTransportMessage(string fileData)
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            var expectedRouteSegment = new RouteSegment
            {
                Mrid = new Guid("52e2fe1f-eb7e-4b33-a412-08527b9b4ed7"),
                Coord = "AQIAACDoZAAAAgAAAMWfDKN8hCBBKSZDQ5SNV0FUVDzX7oQgQWEoq/ufjVdB",
                WorkTaskMrid = Guid.Empty,
                ApplicationName = string.Empty,
                Username = string.Empty
            };

            var expected = new ReceivedLogicalMessage(headers, expectedRouteSegment, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}