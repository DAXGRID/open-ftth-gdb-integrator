using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentInfoCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowException_OnSegmentAfterOrBeforeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);

            // Both being null
            Func<Task> actOne = async () =>
                await factory.Create(null, null);
            await actOne.Should().ThrowExactlyAsync<ArgumentNullException>();

            // Before being null
            Func<Task> actTwo = async () =>
                await factory.Create(null, new RouteSegment());
            await actTwo.Should().ThrowExactlyAsync<ArgumentNullException>();

            // After being null
            Func<Task> actThree = async () =>
                await factory.Create(new RouteSegment(), null);
            await actThree.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnRouteSegmentInfoUpdated_OnUpdatedSegmentInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            var after = new RouteSegment
            {
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Height = "10",
                    Kind = RouteSegmentKindEnum.Indoor,
                    Width = "20"
                }
            };

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteSegmentInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteSegment.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnLifecycleInfoUpdated_OnUpdatedLifecycleInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            var after = new RouteSegment
            {
                LifeCycleInfo = new LifecycleInfo
                {
                    DeploymentState = DeploymentStateEnum.Installed
                }
            };

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteSegmentLifecycleInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteSegment.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnMappingInfoUpdated_OnUpdatedMappingInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            var after = new RouteSegment
            {
                MappingInfo = new MappingInfo
                {
                    HorizontalAccuracy = "10"
                }
            };

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteSegmentMappingInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteSegment.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNamingInfoUpdated_OnUpdatedNamingInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            var after = new RouteSegment
            {
                NamingInfo = new NamingInfo
                {
                    Name = "my test name"
                }
            };

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteSegmentNamingInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteSegment.Should().BeEquivalentTo(after);
            }
        }
    }
}
