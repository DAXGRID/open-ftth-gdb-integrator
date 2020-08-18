using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
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
                LifeCycleInfo = new LifecycleInfo(null, null, null),
                MappingInfo = new MappingInfo(null, null, null, null, null),
                NamingInfo = new NamingInfo(null, null),
                RouteSegmentInfo = new RouteSegmentInfo(null, null, null),
                SafetyInfo = new SafetyInfo(null, null)
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
                LifeCycleInfo = new LifecycleInfo(null, null, null),
                MappingInfo = new MappingInfo(null, null, null, null, null),
                NamingInfo = new NamingInfo(null, null),
                RouteSegmentInfo = new RouteSegmentInfo(null, null, null),
                SafetyInfo = new SafetyInfo(null, null)
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

            A.CallTo(() => serializationMapper.MapDeploymentState("InService")).Returns(DeploymentStateEnum.InService);
            A.CallTo(() => serializationMapper.MapMappingMethod("Drafting")).Returns(MappingMethodEnum.Drafting);
            A.CallTo(() => serializationMapper.MapRouteSegmentKind("Indoor")).Returns(RouteSegmentKindEnum.Indoor);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            RouteSegment expectedRouteSegmentBefore = null;

            var expectedRouteSegmentAfter = new RouteSegment
            {
                Mrid = new Guid("57fb87f5-093c-405d-b619-755e3f39073f"),
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAO79HyV51h/B6DWfEXKJVEGgwmxDUMkfwXuWw252iVRB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2010-06-01T13:45:30"), DateTime.Parse("2010-07-01T13:45:30")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2009-06-01T13:45:30"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Indoor, "20m", "10m"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say"),
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR"
            };

            var expectedMessage = new RouteSegmentMessage(expectedRouteSegmentBefore, expectedRouteSegmentAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedMessage, position);
            var result = routeSegmentSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
