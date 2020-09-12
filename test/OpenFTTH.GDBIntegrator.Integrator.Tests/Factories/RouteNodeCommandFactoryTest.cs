using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Options;
using MediatR;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task CreateDigitizedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.CreateDigitizedEvent(null);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnDoNothing_OnRouteNodeApplicationNameBeingSettingsApplicationName()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            A.CallTo(() => applicationSetting.Value).Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var routeNode = new RouteNode(Guid.Empty, null, Guid.Empty, String.Empty, "GDB_INTEGRATOR");
            var result = (DoNothing)((await factory.CreateDigitizedEvent(routeNode)).First());

            result.Should().BeOfType<DoNothing>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnRouteNodeAdded_OnIntersectingRouteSegmentsCountBeingZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (NewRouteNodeDigitized)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnExistingRouteSegmentSplitted_OnIntersectingRouteSegmentsCountBeingOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = await factory.CreateDigitizedEvent(routeNode);
            var firstEvent = (ExistingRouteSegmentSplitted)result[0];

            using (new AssertionScope())
            {
                firstEvent.RouteNode.Should().BeEquivalentTo(routeNode);
                firstEvent.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnInvalidRouteNodeOperation_OnIntersectingRouteSegmentsCountBeingGreaterThanOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment(), new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (InvalidRouteNodeOperation)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnInvalidRouteNodeOperation_OnIntersectingRouteNodeCountBeingGreaterThanZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var intersectingRouteNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteNodes(routeNode))
                .Returns(intersectingRouteNodes);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (InvalidRouteNodeOperation)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeDeletedEvent_OnRouteNodeMarkAsDeletedSetAndNoIntersectingSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode))
                .Returns(new List<RouteSegment> { });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);
            var result = (RouteNodeDeleted)(await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            using (new AssertionScope())
            {
                result.RouteNode.Should().Be(afterNode);
                result.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNodeOperation_OnRouteNodeMarkAsDeletedSetAndInsectsWithAnyRouteSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(beforeNode))
                .Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });

            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(beforeNode))
                .Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);
            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);

            var expected = new RollbackInvalidRouteNode(beforeNode);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNodeOperation_OnRouteNodeIntersectingWithOtherRouteNodes()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode))
                .Returns(new List<RouteSegment>());

            A.CallTo(() => geoDatabase.GetIntersectingRouteNodes(afterNode))
                .Returns(new List<RouteNode> { A.Fake<RouteNode>() });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);
            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);

            var expected = new RollbackInvalidRouteNode(beforeNode);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnRouteNodeMarkedAsDeletedAndCoordBeingTheSame()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var integratorRouteNode = A.Fake<RouteNode>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());

            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid))
                .Returns(integratorRouteNode);

            A.CallTo(() => afterNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => integratorRouteNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => integratorRouteNode.MarkAsDeleted).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            result.Should().BeOfType<DoNothing>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullArguments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode beforeNode = null;
            RouteNode afterNode = null;

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedBeforeRouteNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode beforeNode = null;
            var afterNode = A.Fake<RouteNode>();

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedAfterNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            RouteNode afterNode = null;

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnShadowTableRouteNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            RouteNode shadowTableRouteNode = null;

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid)).Returns(shadowTableRouteNode);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            result.Should().BeOfType(typeof(DoNothing));
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeLocationChangedAndSplitRouteSegment_OnIntersectingRouteSegment()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid)).Returns(shadowTableRouteNode);

            A.CallTo(() => afterNode.GetGeoJsonCoordinate())
                .Returns("[665931.4446905176,7197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(false);

            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => shadowTableRouteNode.MarkAsDeleted).Returns(false);

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);
            var routeNodeLocationChanged = (RouteNodeLocationChanged)result[0];
            var existingRouteSegmentSplitted = (ExistingRouteSegmentSplitted)result[1];

            using (var scope = new AssertionScope())
            {
                routeNodeLocationChanged.Should().BeOfType(typeof(RouteNodeLocationChanged));
                routeNodeLocationChanged.RouteNodeAfter.Should().Be(afterNode);
                routeNodeLocationChanged.CmdId.Should().NotBeEmpty();

                existingRouteSegmentSplitted.Should().BeOfType(typeof(ExistingRouteSegmentSplitted));
                existingRouteSegmentSplitted.CmdId.Should().NotBeEmpty();
                existingRouteSegmentSplitted.RouteNode.Should().NotBeNull();
                existingRouteSegmentSplitted.RouteSegmentDigitizedByUser.Should().BeNull();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeLocationChanged_OnRouteNodeChangedWithNoChecksFailing()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid)).Returns(shadowTableRouteNode);

            A.CallTo(() => afterNode.GetGeoJsonCoordinate())
                .Returns("[665931.4446905176,7197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(false);

            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => shadowTableRouteNode.MarkAsDeleted).Returns(false);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase);

            var result = (RouteNodeLocationChanged)(await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType(typeof(RouteNodeLocationChanged));
                result.RouteNodeAfter.Should().Be(afterNode);
                result.CmdId.Should().NotBeEmpty();
            }
        }
    }
}
