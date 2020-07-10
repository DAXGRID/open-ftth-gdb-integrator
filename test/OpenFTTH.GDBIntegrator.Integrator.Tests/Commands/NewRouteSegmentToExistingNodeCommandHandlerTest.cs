using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands.Tests.Commands
{
    public class NewRouteSegmentToExistingNodeCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallInsertEndRouteNode_OnBeingCalledWithEndRouteNodeBeingNullAndStartNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = null
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustNotHaveHappened();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldCallInsertStartRouteNode_OnBeingCalledWithStartRouteNodeBeingNullAndEndNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = null,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustHaveHappenedOnceExactly();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustNotHaveHappened();
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowNullArgumentException_OnRouteSegmentBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();

            var command = new NewRouteSegmentToExistingNodeCommand { RouteSegment = null };
            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_OnBothStartAndEndNodeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = A.Fake<RouteSegment>(),
                StartRouteNode = null,
                EndRouteNode = null
            };

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentException>();
        }
    }
}
