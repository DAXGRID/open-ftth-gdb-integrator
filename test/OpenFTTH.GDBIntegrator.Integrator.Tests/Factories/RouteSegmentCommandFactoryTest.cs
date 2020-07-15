using Xunit;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteSegmentCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldReturnNewLonelyRouteSegmentCommand_OnTotalIntersectingNodesCountBeingZero()
        {
            var mediator = A.Fake<IMediator>();

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => mediator
                     .Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>());

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>());

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewLonelyRouteSegmentCommand { RouteSegment = routeSegment };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldReturnNewSegmentBetweenTwoExistingNodesCommand_OnInstersectingStartNodeAndEndNodeCountBeingOneEach()
        {
            var mediator = A.Fake<IMediator>();

            var routeSegment = A.Fake<RouteSegment>();

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>() { new RouteNode() });

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>() { new RouteNode() });

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentBetweenTwoExistingNodesCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = intersectingStartRouteNodes.FirstOrDefault(),
                EndRouteNode = intersectingEndRouteNodes.FirstOrDefault()
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldReturnNewLonelyRouteSegmentCommand_OnInstersectingStartNodeAndEndNodeCountBeingOneEach()
        {
            var mediator = A.Fake<IMediator>();

            var routeSegment = A.Fake<RouteSegment>();

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentBetweenTwoExistingNodesCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = intersectingStartRouteNodes.FirstOrDefault(),
                EndRouteNode = intersectingEndRouteNodes.FirstOrDefault()
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldNewRouteSegmentToExistingNodeCommand_OnInsectingStartNodesCountBeingOneAndEndNodesCountBeingZero()
        {
            var mediator = A.Fake<IMediator>();

            var routeSegment = A.Fake<RouteSegment>();

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { };

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = intersectingStartRouteNodes.FirstOrDefault(),
                EndRouteNode = intersectingEndRouteNodes.FirstOrDefault()
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldNewRouteSegmentToExistingNodeCommand_OnInsectingStartNodesCountBeingZeroAndEndNodesCountBeingOne()
        {
            var mediator = A.Fake<IMediator>();

            var routeSegment = A.Fake<RouteSegment>();

            var intersectingStartRouteNodes = new List<RouteNode> { };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator
                     .Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = intersectingStartRouteNodes.FirstOrDefault(),
                EndRouteNode = intersectingEndRouteNodes.FirstOrDefault()
            };

            result.Should().BeEquivalentTo(expected);
        }
    }
}
