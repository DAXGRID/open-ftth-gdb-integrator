using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests
{
    public class RouteSegmentTest
    {
        [Fact]
        public void RouteSegment_ShouldSetInitialValues_OnConstruction()
        {
            var coord = Convert.FromBase64String("23022322/3232022");
            var mrid = Guid.Parse("053dc6c7-9210-4fbd-b564-f1357bcaf952");
            var username = "gdb-integrator";
            var workTaskMrid = Guid.Parse("8b97d7e6-7d45-4112-b3f3-2209fc3f27d5");
            var applicationName = "gdb-integrator";

            var routeSegment = new RouteSegment
            {
                Coord = coord,
                Mrid = mrid,
                Username = username,
                WorkTaskMrid = workTaskMrid,
                ApplicationName = applicationName
            };

            using (new AssertionScope())
            {
                routeSegment.Coord.Should().BeEquivalentTo(coord);
                routeSegment.Mrid.Should().Be(mrid);
                routeSegment.Username.Should().Be(username);
                routeSegment.WorkTaskMrid.Should().Be(workTaskMrid);
                routeSegment.ApplicationName.Should().Be(applicationName);
            }
        }

        [Fact]
        public void FindStartNode_ShouldReturnNewRouteNodeWithCalculatedCoord_OnBeingCalled()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var startNode = routeSegment.FindStartNode();

            using (new AssertionScope())
            {
                startNode.Mrid.Should().NotBeEmpty();
                startNode.ApplicationName.Should().BeEquivalentTo("GDB_INTEGRATOR");
                startNode.Username.Should().BeEquivalentTo("GDB_INTEGRATOR");
                startNode.WorkTaskMrid.Should().NotBeEmpty();
                startNode.Coord.Should().BeEquivalentTo(Convert.FromBase64String("AQEAAAC8ea7jVkUhQbHPEnAMpFdB"));
            }
        }

        [Fact]
        public void FindEndNode_ShouldReturnNewRouteNodeWithCalculatedCoord_OnBeingCalled()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var endNode = routeSegment.FindEndNode();

            using (new AssertionScope())
            {
                endNode.Mrid.Should().NotBeEmpty();
                endNode.ApplicationName.Should().BeEquivalentTo("GDB_INTEGRATOR");
                endNode.Username.Should().BeEquivalentTo("GDB_INTEGRATOR");
                endNode.WorkTaskMrid.Should().NotBeEmpty();
                endNode.Coord.Should().BeEquivalentTo(Convert.FromBase64String("AQEAAAD97wAWPUUhQbx3w70GpFdB"));
            }
        }

        [Fact]
        public void GetGeoJsonCoordinate_ShouldReturnGeoJsonCoordinate_OnBeingCalled()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var wkbString = routeSegment.GetGeoJsonCoordinate();

            wkbString.Should().BeEquivalentTo("[[565931.4446905176,6197297.75114815],[565962.2423318579,6197319.496780043],[565985.2365167774,6197279.334990998],[565936.8550869693,6197265.913045954],[565943.4099903627,6197290.780060415],[565918.5429759022,6197274.965055402]]");
        }

        [Fact]
    }
}
