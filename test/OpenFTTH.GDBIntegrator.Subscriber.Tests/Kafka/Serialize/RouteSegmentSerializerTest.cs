using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using Topos.Serialization;
using Topos.Consumer;
using System.Collections.Generic;
using FluentAssertions;
using System;
using System.Text;
using FakeItEasy;

namespace OpenFTTH.GDBIntegrator.Subscriber.Tests.Kafka.Serialize
{
    public class RouteSegmentSerializerTest
    {
        [Fact]
        public void Deserialize_ShouldThrowArgumentNullException_OnReceivedLogicalMessageBeingNull()
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeSegmentSerializer = new RouteSegmentSerializer(serializationMapper);

            ReceivedTransportMessage receivedTransportMessage = null;

            routeSegmentSerializer.Invoking(x => x.Deserialize(receivedTransportMessage))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyNull()
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeSegmentSerializer = new RouteSegmentSerializer(serializationMapper);

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
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeSegmentSerializer = new RouteSegmentSerializer(serializationMapper);

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
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeSegmentSerializer = new RouteSegmentSerializer(serializationMapper);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            var expectedRouteSegmentBefore = new RouteSegment
            {
                Mrid = new Guid("57fb87f5-093c-405d-b619-755e3f39073f"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAO79HyV51h/B6DWfEXKJVEGgwmxDUMkfwXuWw252iVRB"),
                WorkTaskMrid = Guid.Empty,
                ApplicationName = string.Empty,
                Username = string.Empty,
                MarkAsDeleted = false,
                ApplicationInfo = string.Empty,
                DeleteMe = false,
            };

            var expectedRouteSegmentAfter = new RouteSegment
            {
                Mrid = new Guid("57fb87f5-093c-405d-b619-755e3f39073f"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAO79HyV51h/B6DWfEXKJVEGgwmxDUMkfwXuWw252iVRB"),
                WorkTaskMrid = Guid.Empty,
                ApplicationName = string.Empty,
                Username = string.Empty,
                MarkAsDeleted = true,
                ApplicationInfo = string.Empty,
                DeleteMe = false,
            };

            var expectedMessage = new RouteSegmentMessage(expectedRouteSegmentBefore, expectedRouteSegmentAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedMessage, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteSegmentSerializerMessageBeforeIsNull.json")]
        public void Deserialize_ShouldReturnDeserializedMessageWithBeforeBeingNull_OnValidReceivedTransportMessageOnBeforeBeingNull(string fileData)
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeSegmentSerializer = new RouteSegmentSerializer(serializationMapper);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            RouteSegment expectedRouteSegmentBefore = null;

            var expectedRouteSegmentAfter = new RouteSegment
            {
                Mrid = new Guid("57fb87f5-093c-405d-b619-755e3f39073f"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAO79HyV51h/B6DWfEXKJVEGgwmxDUMkfwXuWw252iVRB"),
                WorkTaskMrid = Guid.Empty,
                ApplicationName = string.Empty,
                Username = string.Empty,
                MarkAsDeleted = false,
                ApplicationInfo = string.Empty,
                DeleteMe = false,
            };

            var expectedMessage = new RouteSegmentMessage(expectedRouteSegmentBefore, expectedRouteSegmentAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedMessage, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
