using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using System;
using FluentAssertions;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using FluentAssertions.Execution;
using System.Linq;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.Events.Core.Infos;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeInfoCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowException_OnNodeAfterOrBeforeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var factory = new RouteNodeInfoCommandFactory(geoDatabase);

            // Both being null
            Func<Task> actOne = async () =>
                await factory.Create(null, null);
            await actOne.Should().ThrowExactlyAsync<ArgumentNullException>();

            // Before being null
            Func<Task> actTwo = async () =>
                await factory.Create(null, new RouteNode());
            await actTwo.Should().ThrowExactlyAsync<ArgumentNullException>();

            // After being null
            Func<Task> actThree = async () =>
                await factory.Create(new RouteNode(), null);
            await actThree.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnRouteNodeInfoUpdated_OnUpdatedNodeInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            var after = new RouteNode
            {
                RouteNodeInfo = new RouteNodeInfo
                {
                    Function = RouteNodeFunctionEnum.FlexPoint,
                    Kind = RouteNodeKindEnum.CabinetSmall
                }
            };

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteNodeInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteNode.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnLifeCycleInfoUpdated_OnUpdatedLifeCycleInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            var after = new RouteNode
            {
                LifeCycleInfo = new LifecycleInfo
                {
                    InstallationDate = DateTime.UtcNow,
                    DeploymentState = DeploymentStateEnum.NotYetInstalled,
                    RemovalDate = DateTime.UtcNow
                }
            };

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var lifecycleInfoUpdated = (RouteNodeLifecycleInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                lifecycleInfoUpdated.RouteNode.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnMappingInfoUpdated_OnUpdatedMappingInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            var after = new RouteNode
            {
                MappingInfo = new MappingInfo
                {
                    HorizontalAccuracy = "10"
                }
            };

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var mappingInfoUpdated = (RouteNodeMappingInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                mappingInfoUpdated.RouteNode.Should().BeEquivalentTo(after);
            }
        }
    }
}
