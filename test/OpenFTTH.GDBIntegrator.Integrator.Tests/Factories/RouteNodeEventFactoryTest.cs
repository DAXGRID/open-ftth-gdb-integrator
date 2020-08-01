using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeEventFactoryTest
    {
        [Fact]
        public async Task CreateDigitizedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.CreateDigitizedEvent(null);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNull_OnRouteNodeApplicationNameBeingSettingsApplicationName()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            A.CallTo(() => applicationSetting.Value).Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var routeNode = new RouteNode(Guid.Empty, null, Guid.Empty, String.Empty, "GDB_INTEGRATOR");
            var result = await factory.CreateDigitizedEvent(routeNode);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnRouteNodeAdded_OnIntersectingRouteSegmentsCountBeingZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (RouteNodeAdded)(await factory.CreateDigitizedEvent(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
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

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (ExistingRouteSegmentSplitted)(await factory.CreateDigitizedEvent(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldRetrunInvalidRouteNodeOperation_OnIntersectingRouteSegmentsCountBeingGreaterThanOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment(), new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (InvalidRouteNodeOperation)(await factory.CreateDigitizedEvent(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldRetrunInvalidRouteNodeOperation_OnIntersectingRouteNodeCountBeingGreaterThanZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var intersectingRouteNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteNodes(routeNode))
                .Returns(intersectingRouteNodes);

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (InvalidRouteNodeOperation)(await factory.CreateDigitizedEvent(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeRemovedEvent_OnRouteNodeMarkAsDeletedSetAndNoIntersectingSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode))
                .Returns(new List<RouteSegment> { });

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);
            var result = (RouteNodeDeleted)(await factory.CreateUpdatedEvent(beforeNode, afterNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().Be(afterNode);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnNull_OnRouteNodeMarkAsDeletedSetAndInsectsWithAnyRouteSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode))
                .Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);
            var result = (RouteNodeDeleted)(await factory.CreateUpdatedEvent(beforeNode, afterNode));

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnNull_OnRouteNodeMarkedAsDeletedAndCoordBeingTheSame()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var integratorRouteNode = A.Fake<RouteNode>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());

            A.CallTo(() => geoDatabase.GetIntegratorRouteNode(afterNode.Mrid))
                .Returns(integratorRouteNode);

            A.CallTo(() => afterNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => integratorRouteNode.GetGeoJsonCoordinate())
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => integratorRouteNode.MarkAsDeleted).Returns(true);

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);

            result.Should().BeNull();
        }
    }
}
