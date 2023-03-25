using FluentAssertions;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using OpenFTTH.GDBIntegrator.Subscriber.Tests;
using System;
using System.Text.Json;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Subscriber.Postgres;

public class RouteNodeMessageFactoryTest
{
    [Fact]
    public void Create_ShouldThrowArgumentNullException_OnObjectBeingNull()
    {
        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);

        factory.Invoking(x => x.Create(null))
            .Should().Throw<ArgumentNullException>();
    }

    // Here is an example where we try to deserialize a RouteNode
    // but the content is for a RouteSegment so we should fail.
    [Theory]
    [JsonFileData("TestData/RouteSegmentSerializerMessageBeforeIsNull.json")]
    public void Create_ShouldThrowArgumentException_WhenTypeIsNotRouteNode(string json)
    {
        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);
        var editOperation = JsonSerializer.Deserialize<RouteNetworkEditOperation>(json);

        factory.Invoking(x => x.Create(null))
            .Should()
            .Throw<ArgumentException>();
    }

    [Theory]
    [JsonFileData("TestData/RouteNodeSerializerMessageBeforeIsNull.json")]
    public void Create_ShouldReturnRouteNodeMessage_WhenBeforeIsNullAndAfterIsNotNull(string json)
    {
        var expected = new RouteNodeMessage(
            Guid.Parse("6c2c49c9-8c36-4f61-bc41-3eb6f2599b1e"),
            before: null,
            after: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAAABHTNwn4OYgQZg8tZCcnldB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);

        var result = factory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteNodeSerializerMessageAfterIsNull.json")]
    public void Create_ShouldReturnRouteNodeMessage_WhenBeforeIsNotNullAndAfterIsNull(string json)
    {
        var expected = new RouteNodeMessage(
            Guid.Parse("6c2c49c9-8c36-4f61-bc41-3eb6f2599b1e"),
            before: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAAABHTNwn4OYgQZg8tZCcnldB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: null
        );

        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);

        var result = factory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteNodeSerializerMessage.json")]
    public void Create_ShouldReturnRouteNodeMessage_WhenBeforeAndAfterIsSet(string json)
    {
        var expected = new RouteNodeMessage(
            Guid.Parse("6c2c49c9-8c36-4f61-bc41-3eb6f2599b1e"),
            before: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAAABHTNwn4OYgQZg8tZCcnldB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("CD13", "CD13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAAABHTNwn4OYgQZg8tZCcnldB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);

        var result = factory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteNodeSerializerMessageCoordIsNull.json")]
    public void Create_ShouldReturnNodeMessageWithNullCorrd_WhenBeforeAndAfterIsSetAndAfterCoordIsNull(string json)
    {
        var expected = new RouteNodeMessage(
            Guid.Parse("6c2c49c9-8c36-4f61-bc41-3eb6f2599b1e"),
            before: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQEAAABHTNwn4OYgQZg8tZCcnldB"),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("CD13", "CD13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: new RouteNode
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = null,
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteNodeInfo = new RouteNodeInfo(RouteNodeKindEnum.CabinetBig, RouteNodeFunctionEnum.FlexPoint),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var factory = new RouteNodeMessageFactory(infoMapper);

        var result = factory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }
}
