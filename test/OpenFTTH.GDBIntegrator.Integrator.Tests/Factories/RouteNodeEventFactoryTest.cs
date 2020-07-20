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
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            Func<Task> act = async () => await factory.Create(null);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNull_OnRouteNodeApplicationNameBeingSettingsApplicationName()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();

            A.CallTo(() => applicationSetting.Value).Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var routeNode = new RouteNode(Guid.Empty, null, Guid.Empty, String.Empty, "GDB_INTEGRATOR");
            var result = await factory.Create(routeNode);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_ShouldReturnRouteNodeAdded_OnIntersectingRouteSegmentsCountBeingZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (RouteNodeAdded)(await factory.Create(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnExistingRouteSegmentSplittedByUser_OnIntersectingRouteSegmentsCountBeingOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (ExistingRouteSegmentSplittedByUser)(await factory.Create(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
                result.CreatedRouteSegment = routeSegments[0];
            }
        }

        [Fact]
        public async Task Create_ShouldRetrunInvalidRouteNodeOperation_OnIntersectingRouteSegmentsCountBeingGreaterThanOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment(), new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var factory = new RouteNodeEventFactory(applicationSetting, geoDatabase);

            var result = (InvalidRouteNodeOperation)(await factory.Create(routeNode));

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
                result.EventId.Should().NotBeEmpty();
            }
        }
    }
}
