using Xunit;
using NetTopologySuite.Geometries;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
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

            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>());

            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>());

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewLonelyRouteSegment { RouteSegment = routeSegment };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldReturnNewSegmentBetweenTwoExistingNodesCommand_OnInstersectingStartNodeAndEndNodeCountBeingOneEach()
        {
            var mediator = A.Fake<IMediator>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>() { new RouteNode() });

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(new List<RouteNode>() { new RouteNode() });

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentBetweenTwoExistingNodes
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
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentBetweenTwoExistingNodes
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
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { };

            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentToExistingNode
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
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var intersectingStartRouteNodes = new List<RouteNode> { };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new NewRouteSegmentToExistingNode
            {
                RouteSegment = routeSegment,
                StartRouteNode = intersectingStartRouteNodes.FirstOrDefault(),
                EndRouteNode = intersectingEndRouteNodes.FirstOrDefault()
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldReturnInvalidNodeOperationCommand_OnIntersectingRouteSegmentCountBeingBiggerThanOne()
        {
            var mediator = A.Fake<IMediator>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(true);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var intersectingStartRouteNodes = new List<RouteNode> { new RouteNode(), new RouteNode() };
            var intersectingEndRouteNodes = new List<RouteNode> { new RouteNode() };

            A.CallTo(() => mediator.Send(A<GetIntersectingStartRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingStartRouteNodes);

            A.CallTo(() => mediator.Send(A<GetIntersectingEndRouteNodes>._, A<CancellationToken>._))
                .Returns(intersectingEndRouteNodes);

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new InvalidRouteSegmentOperation { RouteSegment = routeSegment };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Create_ShouldReturnInvalidNodeOperationCommand_OnRouteSegmentValidatorReturningFalse()
        {
            var mediator = A.Fake<IMediator>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            A.CallTo(() => routeSegmentValidator.LineIsValid(A<LineString>._)).Returns(false);

            var routeSegment = A.Fake<RouteSegment>();
            A.CallTo(() => routeSegment.GetLineString()).Returns(A.Fake<LineString>());

            var routeSegmentFactory = new RouteSegmentCommandFactory(mediator, routeSegmentValidator);
            var result = await routeSegmentFactory.Create(routeSegment);

            var expected = new InvalidRouteSegmentOperation { RouteSegment = routeSegment };

            result.Should().BeEquivalentTo(expected);
        }
    }
}
