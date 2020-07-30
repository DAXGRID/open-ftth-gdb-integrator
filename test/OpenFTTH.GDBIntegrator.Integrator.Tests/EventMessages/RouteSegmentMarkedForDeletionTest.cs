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
            var eventId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var cmdType = "RouteSegmentDeleted";

            var routeSegmentMarkedForDeletion = new RouteSegmentMarkedForDeletion
                (
                    eventId,
                    segmentId,
                    cmdType
                );

            using (new AssertionScope())
            {
                routeSegmentMarkedForDeletion.CmdId.Should().NotBeEmpty();
                routeSegmentMarkedForDeletion.SegmentId.Should().Be(segmentId);
                routeSegmentMarkedForDeletion.EventId.Should().Be(eventId);
                routeSegmentMarkedForDeletion.EventType.Should().Be(nameof(RouteSegmentMarkedForDeletion));
                routeSegmentMarkedForDeletion.EventTs.Should().NotBeEmpty();
                routeSegmentMarkedForDeletion.CmdType.Should().Be(cmdType);
            }
        }
    }
}
