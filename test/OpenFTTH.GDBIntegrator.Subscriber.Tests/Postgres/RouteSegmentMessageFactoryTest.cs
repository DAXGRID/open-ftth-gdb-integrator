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

public class RouteSegmentMessageFactoryTest
{
    [Fact]
    public void Create_ShouldThrowArgumentNullException_OnObjectBeingNull()
    {
        var infoMapper = new InfoMapper();
        var factory = new RouteSegmentMessageFactory(infoMapper);

        factory.Invoking(x => x.Create(null))
            .Should().Throw<ArgumentNullException>();
    }

    // Here is an example where we try to deserialize a RouteNode
    // but the content is for a RouteNode so we should fail.
    [Theory]
    [JsonFileData("TestData/RouteNodeSerializerMessageBeforeIsNull.json")]
    public void Create_ShouldThrowArgumentException_WhenTypeIsNotRouteSegment(string json)
    {
        var infoMapper = new InfoMapper();
        var factory = new RouteSegmentMessageFactory(infoMapper);
        var editOperation = JsonSerializer.Deserialize<RouteNetworkEditOperation>(json);

        factory.Invoking(x => x.Create(null))
            .Should()
            .Throw<ArgumentException>();
    }

    [Theory]
    [JsonFileData("TestData/RouteSegmentSerializerMessageBeforeIsNull.json")]
    public void Create_ShouldReturnRouteSegmentMessage_WhenBeforeIsNullAndAfterIsNotNull(string json)
    {
        var expected = new RouteSegmentMessage(
            Guid.Parse("88ce4357-6cc8-4c02-a652-d3272693c4b8"),
            before: null,
            after: new RouteSegment
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQIAAAACAAAALBxprVDnIEFFb8jPiZ5XQUUMFXHn5yBBRW/Iz4meV0E="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var messageFactory = new RouteSegmentMessageFactory(infoMapper);

        var result = messageFactory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteSegmentSerializerMessageAfterIsNull.json")]
    public void Create_ShouldReturnRouteSegmentMessage_WhenBeforeIsNotNullAndAfterIsNull(string json)
    {
        var expected = new RouteSegmentMessage(
            Guid.Parse("88ce4357-6cc8-4c02-a652-d3272693c4b8"),
            before: new RouteSegment
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQIAAAACAAAALBxprVDnIEFFb8jPiZ5XQUUMFXHn5yBBRW/Iz4meV0E="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: null
        );

        var infoMapper = new InfoMapper();
        var messageFactory = new RouteSegmentMessageFactory(infoMapper);

        var result = messageFactory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteSegmentSerializerMessage.json")]
    public void Create_ShouldReturnRouteSegmentMessage_WhenBeforeAndAfterIsSet(string json)
    {
        var expected = new RouteSegmentMessage(
            Guid.Parse("88ce4357-6cc8-4c02-a652-d3272693c4b8"),
            before: new RouteSegment
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQIAAAACAAAALBxprVDnIEFFb8jPiZ5XQUUMFXHn5yBBRW/Iz4meV0E="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: new RouteSegment
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQIAAAACAAAALBxprVDnIEFFb8jPiZ5XQUUMFXHn5yBBRW/Iz4meV0E="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("FF13", "FF13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var messageFactory = new RouteSegmentMessageFactory(infoMapper);

        var result = messageFactory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }

    [Theory]
    [JsonFileData("TestData/RouteSegmentSerializerMessageCoordIsNull.json")]
    public void Create_ShouldReturnRouteSegmentMessageWithNullCorrd_WhenBeforeAndAfterIsSetAndAfterCoordIsNull(string json)
    {
        var expected = new RouteSegmentMessage(
            Guid.Parse("88ce4357-6cc8-4c02-a652-d3272693c4b8"),
            before: new RouteSegment
            {
                ApplicationInfo = "Application information bla bla",
                ApplicationName = "GDB_INTEGRATOR",
                Coord = Convert.FromBase64String("AQIAAAACAAAALBxprVDnIEFFb8jPiZ5XQUUMFXHn5yBBRW/Iz4meV0E="),
                DeleteMe = false,
                MarkAsDeleted = false,
                Mrid = new Guid("66563e62-db0e-4184-be75-d55638bf33a5"),
                Username = "Rune Nielsen",
                WorkTaskMrid = Guid.Parse("d80f6250-8132-4893-b38f-b64fafd74316"),
                LifeCycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Parse("2020-08-12 00:00:00"), DateTime.Parse("2020-08-12 00:00:00")),
                MappingInfo = new MappingInfo(MappingMethodEnum.Drafting, "1", "2", DateTime.Parse("2020-08-12 00:00:00"), "Source info"),
                NamingInfo = new NamingInfo("AB13", "AB13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            },
            after: new RouteSegment
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
                NamingInfo = new NamingInfo("FF13", "FF13 desc"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Arial, "10", "20"),
                SafetyInfo = new SafetyInfo("Very safe", "Nothing to say")
            }
        );

        var infoMapper = new InfoMapper();
        var messageFactory = new RouteSegmentMessageFactory(infoMapper);

        var result = messageFactory.Create(
            JsonSerializer.Deserialize<RouteNetworkEditOperation>(json));

        result.EventId.Should().Be(expected.EventId);
        result.Before.Should().BeEquivalentTo(expected.Before);
        result.After.Should().BeEquivalentTo(expected.After);
    }
}
