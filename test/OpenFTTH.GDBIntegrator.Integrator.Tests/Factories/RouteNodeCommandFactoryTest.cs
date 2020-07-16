using Xunit;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedRouteNodeThatIsNull()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode routeNode = null;
            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase);

            Func<Task> act = async () => { await routeNodeCommandFactory.Create(routeNode); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNewLonelyRouteNode_OnIntersectingRouteSegmentsBeingZero()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode routeNode = A.Fake<RouteNode>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase);

            var result = await routeNodeCommandFactory.Create(routeNode);

            var expected = new NewLonelyRouteNodeCommand { RouteNode = routeNode };
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShowThrowException_OnIntersectingRouteSegmentsBeingBiggerThanZero()
        {
            var mediator = A.Fake<IMediator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode routeNode = A.Fake<RouteNode>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>() { new RouteSegment() });

            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator, geoDatabase);

            Func<Task> act = async () => await routeNodeCommandFactory.Create(routeNode);

            await act.Should().ThrowExactlyAsync<Exception>();
        }
    }
}
