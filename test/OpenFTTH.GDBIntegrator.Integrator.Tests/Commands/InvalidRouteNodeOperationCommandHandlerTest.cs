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
    public class InvalidRouteNodeOperationHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallGeoDatabaseDelete_OnBeingSuppliedRouteNode()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<InvalidRouteNodeOperationHandler>>();
            var routeNode = A.Fake<RouteNode>();

            var request = new InvalidRouteNodeOperation { RouteNode = routeNode };

            var invalidRouteNodeOperationHandler = new InvalidRouteNodeOperationHandler(geoDatabase, logger);
            await invalidRouteNodeOperationHandler.Handle(request, new CancellationToken());

            A.CallTo(() => geoDatabase.DeleteRouteNode(routeNode.Mrid)).MustHaveHappenedOnceExactly();
        }
    }
}
