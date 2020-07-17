using Xunit;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class InvalidRouteNodeOperationCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallGeoDatabaseDelete_OnBeingSuppliedRouteNode()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<InvalidRouteNodeOperationCommandHandler>>();
            var routeNode = A.Fake<RouteNode>();

            var request = new InvalidRouteNodeOperationCommand { RouteNode = routeNode };

            var invalidRouteNodeOperationHandler = new InvalidRouteNodeOperationCommandHandler(geoDatabase, logger);
            await invalidRouteNodeOperationHandler.Handle(request, new CancellationToken());

            A.CallTo(() => geoDatabase.DeleteRouteNode(routeNode.Mrid)).MustHaveHappenedOnceExactly();
        }
    }
}
