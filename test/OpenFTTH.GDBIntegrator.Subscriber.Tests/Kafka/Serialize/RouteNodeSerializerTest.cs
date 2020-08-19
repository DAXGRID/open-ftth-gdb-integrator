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
    public class RouteNodeSerializerTest
    {
        [Fact]
        public void Deserialize_ShouldThrowArgumentNullException_OnReceivedLogicalMessageBeingNull()
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeNodeSerializer = new RouteNodeSerializer(serializationMapper);

            ReceivedTransportMessage receivedTransportMessage = null;

            routeNodeSerializer.Invoking(x => x.Deserialize(receivedTransportMessage))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyNull()
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeNodeSerializer = new RouteNodeSerializer(serializationMapper);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            byte[] body = null;

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);
            var expected = new ReceivedLogicalMessage(headers, new RouteNodeMessage(), position);

            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Deserialize_ShouldReturnEmptyReceivedLogicalMessage_OnMessageBodyLengthIsZero()
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeNodeSerializer = new RouteNodeSerializer(serializationMapper);

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
            var serializationMapper = A.Fake<ISerializationMapper>();

            A.CallTo(() => serializationMapper.MapDeploymentState("InService")).Returns(DeploymentStateEnum.InService);
            A.CallTo(() => serializationMapper.MapMappingMethod("Drafting")).Returns(MappingMethodEnum.Drafting);
            A.CallTo(() => serializationMapper.MapRouteNodeKind("CabinetBig")).Returns(RouteNodeKindEnum.CabinetBig);
            A.CallTo(() => serializationMapper.MapRouteNodeFunction("FlexPoint")).Returns(RouteNodeFunctionEnum.FlexPoint);

            var routeNodeSerializer = new RouteNodeSerializer(serializationMapper);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            RouteNode expectedRouteNodeBefore = null;

            var expectedRouteNodeAfter = new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAACDoZAAADEpxfJCIIUFJI+ZYZL1XQQ=="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2010-06-01T13:45:30"), DateTime.Parse("2010-07-01T13:45:30")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2009-06-01T13:45:30"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            };

            var expectedBody = new RouteNodeMessage(expectedRouteNodeBefore, expectedRouteNodeAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedBody, position);
            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [JsonFileData("TestData/RouteNodeSerializerMessage.json")]
        public void Deserialize_ShouldReturnDeserializedMessage_OnValidReceivedTransportMessage(string fileData)
        {
            var serializationMapper = A.Fake<ISerializationMapper>();
            var routeNodeSerializer = new RouteNodeSerializer(serializationMapper);
            A.CallTo(() => serializationMapper.MapDeploymentState("InService")).Returns(DeploymentStateEnum.InService);
            A.CallTo(() => serializationMapper.MapRouteNodeFunction("FlexPoint")).Returns(RouteNodeFunctionEnum.FlexPoint);

            var position = new Position();
            var headers = new Dictionary<string, string>();
            var body = Encoding.UTF8.GetBytes(fileData);

            var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

            var expectedRouteNodeBefore = new RouteNode
            {
                ApplicationInfo = string.Empty,
                ApplicationName = string.Empty,
                Coord = Convert.FromBase64String("AQEAACDoZAAAqxoVSPa2H8GsStinzINUQQ=="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("9bffa519-c672-49fd-93d0-52cd22519346"),
                Username = string.Empty,
                WorkTaskMrid = Guid.Empty,
                LifeCycleInfo = null,
                MappingInfo = null,
                NamingInfo = null,
                RouteNodeInfo = null,
                SafetyInfo = null
            };

            var expectedRouteNodeAfter = new RouteNode
            {
                ApplicationInfo = string.Empty,
                ApplicationName = string.Empty,
                Coord = Convert.FromBase64String("AQEAACDoZAAAqxoVSPa2H8GsStinzINUQQ=="),
                DeleteMe = false,
                MarkAsDeleted = true,
                Mrid = new Guid("9bffa519-c672-49fd-93d0-52cd22519346"),
                Username = string.Empty,
                WorkTaskMrid = Guid.Empty,
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2010-06-01T13:45:30"), DateTime.Parse("2010-07-01T13:45:30")),
                MappingInfo = null,
                NamingInfo = null,
                RouteNodeInfo = new RouteNodeInfo(null, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = null
            };

            var expectedBody = new RouteNodeMessage(expectedRouteNodeBefore, expectedRouteNodeAfter);

            var expected = new ReceivedLogicalMessage(headers, expectedBody, position);
            var result = routeNodeSerializer.Deserialize(receivedTransportMessage);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
