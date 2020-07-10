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

namespace OpenFTTH.GDBIntegrator.Integrator.Commands.Tests
{
    public class NewLonelyRouteSegmentCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallInsertTwoRouteNodes_OnBeingCalledWithRouteSegment()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewLonelyRouteSegmentCommandHandler>>();

            var commandHandler = new NewLonelyRouteSegmentCommandHandler(geoDatabase, logger);

            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();
            var routeSegment = A.Dummy<RouteSegment>();
            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);

            var command = new NewLonelyRouteSegmentCommand { RouteSegment = routeSegment };
            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustHaveHappenedOnceExactly();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowNullArgumentException_OnRouteSegmentBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewLonelyRouteSegmentCommandHandler>>();

            var command = new NewLonelyRouteSegmentCommand { RouteSegment = null };
            var commandHandler = new NewLonelyRouteSegmentCommandHandler(geoDatabase, logger);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
