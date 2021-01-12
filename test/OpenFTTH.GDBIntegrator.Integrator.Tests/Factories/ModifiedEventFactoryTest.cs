using System;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class ModifiedEventFactoryTest
    {
        [Fact]
        public void CreateRouteSegmentInfoModified_ShouldReturnsEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var segmentId = Guid.NewGuid();

            var routeSegment = new RouteSegment
            {
                Mrid = segmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Width = "10cm",
                    Height = "2cm",
                    Kind = (RouteSegmentKindEnum?)RouteNodeKindEnum.BuildingAccessPoint
                },
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
            };

            var result = modifiedEventFactory.CreateRouteSegmentInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be(nameof(RouteSegmentInfoModified));
                result.EventId.Should().NotBeEmpty();
                result.SegmentId.Should().Be(segmentId);
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                // Should not be default datetime
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.RouteSegmentInfo.Should().BeEquivalentTo(routeSegment.RouteSegmentInfo);
            }
        }
    }
}
