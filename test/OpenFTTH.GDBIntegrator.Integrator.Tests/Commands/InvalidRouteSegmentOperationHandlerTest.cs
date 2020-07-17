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
    public class InvalidRouteSegmentOperationHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallGeoDatabaseDeleteRouteSegment_OnBeingSuppliedRouteSegment()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<InvalidRouteSegmentOperationHandler>>();
            var routeSegment = A.Fake<RouteSegment>();

            var request = new InvalidRouteSegmentOperation { RouteSegment = routeSegment };

            var invalidRouteSegmentOperationHandler = new InvalidRouteSegmentOperationHandler(geoDatabase, logger);
            await invalidRouteSegmentOperationHandler.Handle(request, new CancellationToken());

            A.CallTo(() => geoDatabase.DeleteRouteSegment(routeSegment.Mrid)).MustHaveHappenedOnceExactly();
        }
    }
}
