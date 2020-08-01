using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteSegmentMarkedForDeletionTest
    {
        [Fact]
        public void RouteSegmentMarkedForDeletion_ShouldSetInitialValues_OnConstruction()
        {
            var cmdId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var cmdType = "RouteSegmentDeleted";

            var routeSegmentMarkedForDeletion = new RouteSegmentMarkedForDeletion
                (
                    cmdId,
                    segmentId,
                    cmdType
                );

            using (new AssertionScope())
            {
                routeSegmentMarkedForDeletion.EventId.Should().NotBeEmpty();
                routeSegmentMarkedForDeletion.SegmentId.Should().Be(segmentId);
                routeSegmentMarkedForDeletion.CmdId.Should().Be(cmdId);
                routeSegmentMarkedForDeletion.EventType.Should().Be(nameof(RouteSegmentMarkedForDeletion));
                routeSegmentMarkedForDeletion.EventTs.Should().NotBeEmpty();
                routeSegmentMarkedForDeletion.CmdType.Should().Be(cmdType);
            }
        }
    }
}
