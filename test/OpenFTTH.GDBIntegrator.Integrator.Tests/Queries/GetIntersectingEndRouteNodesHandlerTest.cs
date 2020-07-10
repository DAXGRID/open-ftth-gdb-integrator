using Xunit;
using FluentAssertions;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Queries.Test.Queries
{
    public class GetIntersectingEndRouteNodesHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldReturnEndRouteNode_OnRequest()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();

            var expected = A.Fake<List<RouteNode>>();
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(A.Fake<RouteSegment>())).Returns(expected);

            var command = new GetIntersectingEndRouteNodes();
            var handler = new GetIntersectingEndRouteNodesHandler(geoDatabase);

            var result = await handler.Handle(command, new CancellationToken());

            result.Should().BeEquivalentTo(expected);
        }
    }
}
