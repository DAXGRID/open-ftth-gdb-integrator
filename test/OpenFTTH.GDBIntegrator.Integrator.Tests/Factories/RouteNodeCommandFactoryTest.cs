using Xunit;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using MediatR;
using System.Threading.Tasks;
using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedRouteNodeThatIsNull()
        {
            var mediator = A.Fake<IMediator>();
            RouteNode routeNode = null;
            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator);

            Func<Task> act = async () => { await routeNodeCommandFactory.Create(routeNode); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNewLonelyRouteNode_OnIntersectingRouteSegmentsBeingZero()
        {
            var mediator = A.Fake<IMediator>();
            RouteNode routeNode = A.Fake<RouteNode>();
            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator);

            var result = await routeNodeCommandFactory.Create(routeNode);

            var expected = new NewLonelyRouteNodeCommand { RouteNode = routeNode };

            result.Should().BeEquivalentTo(expected);
        }
    }
}
