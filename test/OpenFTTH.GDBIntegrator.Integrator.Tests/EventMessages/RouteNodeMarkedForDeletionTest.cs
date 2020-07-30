using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.EventMessages
{
    public class RouteNodeMarkedForDeletionTest
    {
        [Fact]
        public void RouteNodeMarkedForDeletion_ShouldSetInitialValues_OnConstruction()
        {
            var eventId = Guid.NewGuid();
            var nodeId = Guid.NewGuid();
            var cmdType = "RouteNodeDeleted";

            var routeNodeMarkedForDeletion = new RouteNodeMarkedForDeletion
                (
                    eventId,
                    nodeId,
                    cmdType
                );

            using (new AssertionScope())
            {
                routeNodeMarkedForDeletion.CmdId.Should().NotBeEmpty();
                routeNodeMarkedForDeletion.NodeId.Should().Be(nodeId);
                routeNodeMarkedForDeletion.EventId.Should().Be(eventId);
                routeNodeMarkedForDeletion.EventType.Should().Be(nameof(RouteNodeMarkedForDeletion));
                routeNodeMarkedForDeletion.EventTs.Should().NotBeEmpty();
                routeNodeMarkedForDeletion.CmdType.Should().Be(cmdType);
            }
        }
    }
}
