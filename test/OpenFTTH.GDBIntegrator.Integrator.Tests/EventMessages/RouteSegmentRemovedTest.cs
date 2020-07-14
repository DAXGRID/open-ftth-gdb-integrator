using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteSegmentRemovedTest
    {
        [Fact]
        public void RouteSegmentRemoved_ShouldSetInitialValues_OnConstruction()
        {
            var eventId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var replacedBySegments = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var routeSegment = new RouteSegmentRemoved(eventId, segmentId, replacedBySegments);

            using (new AssertionScope())
            {
                routeSegment.CmdId.Should().NotBeEmpty();
                routeSegment.EventId.Should().Be(eventId);
                routeSegment.EventType.Should().Be("RouteSegmentRemoved");
                routeSegment.EventTs.Should().NotBeEmpty();
                routeSegment.ReplacedBySegments.Should().BeEquivalentTo(replacedBySegments);
                routeSegment.SegmentId.Should().Be(segmentId);
            }
        }

        [Fact]
        public void RouteSegmentRemoved_ShouldThrowArgumentOutOfRangeException_OnReplacedBySegmentCountBeingGreaterThanTwo()
        {
            var eventId = Guid.NewGuid();
            var segmentId = Guid.NewGuid();
            var replacedBySegments = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            Action act = () => new RouteSegmentRemoved(eventId, segmentId, replacedBySegments);

            act.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }
    }
}
