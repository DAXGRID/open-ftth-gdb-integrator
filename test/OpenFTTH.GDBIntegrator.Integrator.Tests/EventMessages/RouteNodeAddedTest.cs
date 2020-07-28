using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteNodeAddedTest
    {
        [Fact]
        public void RouteNodeAdded_ShouldSetInitialValues_OnConstruction()
        {
            var eventId = Guid.NewGuid();
            var nodeId = Guid.NewGuid();
            var geometry = "[565931.44469051762,6197297.75114815]";
            var cmdType = "RouteNodeDigitized";

            var routeSegment = new RouteNodeAdded
                (
                    eventId,
                    nodeId,
                    geometry,
                    cmdType
                );

            using (new AssertionScope())
            {
                routeSegment.Geometry.Should().Be(geometry);
                routeSegment.CmdId.Should().NotBeEmpty();
                routeSegment.NodeId.Should().Be(nodeId);
                routeSegment.EventId.Should().Be(eventId);
                routeSegment.EventType.Should().Be("RouteNodeAdded");
                routeSegment.EventTs.Should().NotBeEmpty();
                routeSegment.CmdType.Should().Be(cmdType);
            }
        }
    }
}
