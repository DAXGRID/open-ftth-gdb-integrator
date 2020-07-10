using System;
using Xunit;
using FluentAssertions;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands.Tests
{
    public class NewRouteSegmentBetweenTwoExistingNodesCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallAndNotInsertAny_OnBeingCalledWithRouteSegment()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler>>();

            var commandHandler = new NewRouteSegmentBetweenTwoExistingNodesCommandHandler(geoDatabase, logger);
            var routeSegment = A.Fake<RouteSegment>();

            var command = new NewRouteSegmentBetweenTwoExistingNodesCommand { RouteSegment = routeSegment };

            var result = await commandHandler.Handle(command, new CancellationToken());
            result.Should().NotBeNull();
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
