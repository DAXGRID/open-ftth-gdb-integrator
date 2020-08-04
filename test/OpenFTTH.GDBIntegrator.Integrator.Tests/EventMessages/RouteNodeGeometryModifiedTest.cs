using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteNodeGeometryModifiedTest
    {
        [Fact]
        public void RouteNodeGeometryModified_ShouldSetInitialValues_OnConstruction()
        {
            var cmdId = Guid.NewGuid();
            var nodeId = Guid.NewGuid();
            var geometry = "[565931.444690517626197297.75114815]";
            var cmdType = "RouteSegmentLocationChanged";

            var routeNodeGeometryModified = new RouteNodeGeometryModified
                (
                    cmdId,
                    nodeId,
                    cmdType,
                    geometry
                );

            using (new AssertionScope())
            {
                routeNodeGeometryModified.Geometry.Should().Be(geometry);
                routeNodeGeometryModified.EventId.Should().NotBeEmpty();
                routeNodeGeometryModified.NodeId.Should().Be(nodeId);
                routeNodeGeometryModified.CmdId.Should().Be(cmdId);
                routeNodeGeometryModified.EventType.Should().Be(nameof(RouteSegmentGeometryModified));
                routeNodeGeometryModified.EventTs.Should().NotBeEmpty();
                routeNodeGeometryModified.CmdType.Should().Be(cmdType);
            }
        }
    }
}
