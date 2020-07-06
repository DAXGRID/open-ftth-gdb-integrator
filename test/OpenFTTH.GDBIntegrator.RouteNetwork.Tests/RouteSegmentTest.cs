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
            var coord = "23022322/3232022";
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
                ApplicationName = applicationName,
            };

            using (new AssertionScope())
            {
                routeSegment.Coord.Should().Be(coord);
                routeSegment.Mrid.Should().Be(mrid);
                routeSegment.Username.Should().Be(username);
                routeSegment.WorkTaskMrid.Should().Be(workTaskMrid);
                routeSegment.ApplicationName.Should().Be(applicationName);
            }
        }
    }
}
