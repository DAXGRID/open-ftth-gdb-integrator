using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteSegmentGeometryModifiedTest
    {
        [Fact]
        public void RouteSegmentGeoemtryModified_ShouldSetInitialValues_OnConstruction()
        {
            var cmdId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var geometry = "[565931.444690517626197297.75114815,565962.242331857916197319.4967800425,565985.236516777436197279.3349909978,"
                + "565936.855086969336197265.913045954,565943.409990362716197290.7800604152,565918.542975902216197274.9650554024]";
            var cmdType = "RouteSegmentLocationChanged";

            var routeSegmentGeometryModified = new RouteSegmentGeometryModified
                (
                    cmdId,
                    segmentId,
                    cmdType,
                    geometry
                );

            using (new AssertionScope())
            {
                routeSegmentGeometryModified.Geometry.Should().Be(geometry);
                routeSegmentGeometryModified.EventId.Should().NotBeEmpty();
                routeSegmentGeometryModified.SegmentId.Should().Be(segmentId);
                routeSegmentGeometryModified.CmdId.Should().Be(cmdId);
                routeSegmentGeometryModified.EventType.Should().Be(nameof(RouteSegmentGeometryModified));
                routeSegmentGeometryModified.EventTs.Should().NotBeEmpty();
                routeSegmentGeometryModified.CmdType.Should().Be(cmdType);
            }
        }
    }
}
