using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Messages;
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

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);
            var expected = new ReceivedLogicalMessage(headers, new RouteSegmentMessage(), position);

            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyLengthIsZero()
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = new byte[0];

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, null);
            var expected = new ReceivedLogicalMessage(headers, new RouteSegmentMessage(), position);

            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

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

            var expectedRouteSegmentBefore = new RouteSegment
            {
                Mrid = new Guid("bfa35619-e60c-4393-99d1-fb21abce702e"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAM8G4/JefCBBjzUaLet5V0HDwDo5pH0gQdhEy5rqeVdB"),
                WorkTaskMrid = Guid.Empty,
                ApplicationName = string.Empty,
                Username = string.Empty
            };

            var expectedRouteSegmentAfter = new RouteSegment
            {
                Mrid = new Guid("bfa35619-e60c-4393-99d1-fb21abce702e"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAM8G3/JefCBBjzUaLet5V0HDwDo5pH0gQdhEy5rqeVdB"),
                WorkTaskMrid = new Guid("bfa35619-e60c-4393-99d1-fb21abce702f"),
                ApplicationName = "GDB_INTEGRATOR",
                Username = "GDB_INTEGRATOR"
            };

            var expectedMessage = new RouteSegmentMessage(expectedRouteSegmentBefore, expectedRouteSegmentAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedMessage, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteSegmentSerializerMessageBeforeIsNull.json")]
        public void Deserialize_ShouldReturnDeserializedMessageWithBeforeBeingNull_OnValidReceivedTransportMessage(string fileData)
        {
            var routeSegmentSerializer = new RouteSegmentSerializer();

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            RouteSegment expectedRouteSegmentBefore = null;

            var expectedRouteSegmentAfter = new RouteSegment
            {
                Mrid = new Guid("bfa35619-e60c-4393-99d1-fb21abce702e"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAM8G3/JefCBBjzUaLet5V0HDwDo5pH0gQdhEy5rqeVdB"),
                WorkTaskMrid = new Guid("bfa35619-e60c-4393-99d1-fb21abce702f"),
                ApplicationName = "GDB_INTEGRATOR",
                Username = "GDB_INTEGRATOR"
            };

            var expectedMessage = new RouteSegmentMessage(expectedRouteSegmentBefore, expectedRouteSegmentAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedMessage, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
