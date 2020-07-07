using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Queries.Test
{
    public class GetIntersectingRoutesNodesTest
    {
        [Fact]
        public async Task GetIntersectingRouteNodes_ShouldReturnIntersectingRouteNodes_OnBeingPassedRouteNode()
        {
            var request = new GetIntersectingRouteNodes();
            var getNetworkAtPositionHandler = new GetIntersectingRouteNodesHandler();
            var result = await getNetworkAtPositionHandler.Handle(request, new CancellationToken());

            result.Should().BeOfType<List<RouteNode>>();
        }
    }
}
