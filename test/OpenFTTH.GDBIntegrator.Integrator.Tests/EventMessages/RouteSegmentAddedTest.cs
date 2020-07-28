using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteSegmentAddedTest
    {
        [Fact]
        public void RouteSegmentAdded_ShouldSetInitialValues_OnConstruction()
        {
            var eventId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var fromNodeId = Guid.NewGuid();
            var toNodeId = Guid.NewGuid();
            var geometry = "[565931.444690517626197297.75114815,565962.242331857916197319.4967800425,565985.236516777436197279.3349909978,"
                + "565936.855086969336197265.913045954,565943.409990362716197290.7800604152,565918.542975902216197274.9650554024]";
            var cmdType = "RouteSegmentDigitized";

            var routeSegmentAdded = new RouteSegmentAdded
                (
                    eventId,
                    segmentId,
                    fromNodeId,
                    toNodeId,
                    geometry,
                    cmdType
                );

            using (new AssertionScope())
            {
                routeSegmentAdded.Geometry.Should().Be(geometry);
                routeSegmentAdded.CmdId.Should().NotBeEmpty();
                routeSegmentAdded.SegmentId.Should().Be(segmentId);
                routeSegmentAdded.FromNodeId.Should().Be(fromNodeId);
                routeSegmentAdded.ToNodeId.Should().Be(toNodeId);
                routeSegmentAdded.EventId.Should().Be(eventId);
                routeSegmentAdded.EventType.Should().Be("RouteSegmentAdded");
                routeSegmentAdded.EventTs.Should().NotBeEmpty();
                routeSegmentAdded.CmdType.Should().Be(cmdType);
            }
        }
    }
}
