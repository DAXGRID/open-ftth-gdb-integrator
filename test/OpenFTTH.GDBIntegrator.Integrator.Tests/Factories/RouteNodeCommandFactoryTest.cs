using Xunit;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Config;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedRouteNodeThatIsNull()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            RouteNode routeNode = null;
            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase, applicationSetting);

            Func<Task> act = async () => { await routeNodeCommandFactory.Create(routeNode); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNewLonelyRouteNode_OnIntersectingRouteSegmentsBeingZero()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode routeNode = A.Fake<RouteNode>();
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase, applicationSetting);

            var result = await routeNodeCommandFactory.Create(routeNode);

            var expected = new NewLonelyRouteNodeCommand { RouteNode = routeNode };
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ThrowException_OnIntersectingRouteSegmentsBeingBiggerThanZero()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode routeNode = A.Fake<RouteNode>();
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>() { new RouteSegment() });

            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase, applicationSetting);

            Func<Task> act = async () => await routeNodeCommandFactory.Create(routeNode);

            await act.Should().ThrowExactlyAsync<Exception>();
        }

        [Fact]
        public async Task Create_ShouldReturnGdbCreatedEntityCommand_OnUsernameBeingApplicationNameForGdbIntegrator()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();

            A.CallTo(() => routeNode.ApplicationName).Returns("GDB_INTEGRATOR");

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>() { new RouteSegment() });

            A.CallTo(() => applicationSetting.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase, applicationSetting);

            var result = await routeNodeCommandFactory.Create(routeNode);

            result.Should().BeOfType(typeof(GdbCreatedEntityCommand));
        }
    }
}
