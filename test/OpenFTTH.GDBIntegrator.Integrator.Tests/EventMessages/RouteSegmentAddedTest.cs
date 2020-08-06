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
            var cmdId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var fromNodeId = Guid.NewGuid();
            var toNodeId = Guid.NewGuid();
            var geometry = "[565931.444690517626197297.75114815,565962.242331857916197319.4967800425,565985.236516777436197279.3349909978,"
                + "565936.855086969336197265.913045954,565943.409990362716197290.7800604152,565918.542975902216197274.9650554024]";
            var cmdType = "RouteSegmentDigitized";
            var segmentKind = "Kind";
            var workTaskMrid = Guid.NewGuid();
            var username = "user123";
            var applicationName = "GDB_INTEGRATOR";
            var applicationInfo = "GDB_INTEGRATOR made this";

            var routeSegmentAdded = new RouteSegmentAdded
                (
                    cmdId,
                    segmentId,
                    fromNodeId,
                    toNodeId,
                    geometry,
                    cmdType,
                    segmentKind,
                    workTaskMrid,
                    username,
                    applicationName,
                    applicationInfo
                );

            using (new AssertionScope())
            {
                routeSegmentAdded.Geometry.Should().Be(geometry);
                routeSegmentAdded.EventId.Should().NotBeEmpty();
                routeSegmentAdded.SegmentId.Should().Be(segmentId);
                routeSegmentAdded.FromNodeId.Should().Be(fromNodeId);
                routeSegmentAdded.ToNodeId.Should().Be(toNodeId);
                routeSegmentAdded.CmdId.Should().Be(cmdId);
                routeSegmentAdded.EventType.Should().Be("RouteSegmentAdded");
                routeSegmentAdded.EventTs.Should().NotBeEmpty();
                routeSegmentAdded.CmdType.Should().Be(cmdType);
                routeSegmentAdded.SegmentKind.Should().Be(segmentKind);
                routeSegmentAdded.WorkTaskMrid.Should().Be(workTaskMrid);
                routeSegmentAdded.Username.Should().Be(username);
                routeSegmentAdded.ApplicationName.Should().Be(applicationName);
                routeSegmentAdded.ApplicationInfo.Should().Be(applicationInfo);
            }
        }
    }
}
