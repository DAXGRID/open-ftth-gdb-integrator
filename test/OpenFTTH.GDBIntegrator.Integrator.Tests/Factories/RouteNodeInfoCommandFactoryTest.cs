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
        public async Task Create_ShouldReturnRollBackInvalidRouteNode_OnAfterBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            RouteNode after = null;

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var rollbackEvent = (RollbackInvalidRouteNode)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                rollbackEvent.Should().BeOfType(typeof(RollbackInvalidRouteNode));
            }
        }

        [Fact]
        public async Task Create_ShouldReturnRollBackInvalidRouteNode_OnBeforeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode before = null;
            RouteNode after = new RouteNode();

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var rollbackEvent = (RollbackInvalidRouteNode)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                rollbackEvent.Should().BeOfType(typeof(RollbackInvalidRouteNode));
            }
        }

        [Fact]
        public async Task Create_ShouldReturnDoNothing_OnShadowTableRouteNodeBeingEqToAfter()
        {
            var routeNodeId = Guid.NewGuid();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode
            {
                Mrid = routeNodeId,
                RouteNodeInfo = new RouteNodeInfo
                {
                    Kind = RouteNodeKindEnum.CabinetBig
                }
            };
            var after = new RouteNode
            {
                Mrid = routeNodeId,
                RouteNodeInfo = new RouteNodeInfo
                {
                    Kind = RouteNodeKindEnum.CabinetBig
                }
            };
            var shadowTableSegment = new RouteNode
            {
                Mrid = routeNodeId,
                RouteNodeInfo = new RouteNodeInfo
                {
                    Kind = RouteNodeKindEnum.CabinetBig
                }
            };

            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(after.Mrid, true)).Returns(shadowTableSegment);

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var doNothingEvent = (DoNothing)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                doNothingEvent.Should().BeOfType(typeof(DoNothing));
            }
        }

        [Fact]
        public async Task Create_ShouldRollback_OnBeforeBeingMarkedAsDeleted()
        {
            var routeNodeId = Guid.NewGuid();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode
            {
                Mrid = routeNodeId,
                MarkAsDeleted = true
            };
            var after = new RouteNode
            {
                Mrid = routeNodeId,
                RouteNodeInfo = new RouteNodeInfo
                {
                    Kind = RouteNodeKindEnum.CabinetBig
                }
            };
            var shadowTableSegment = new RouteNode
            {
                Mrid = routeNodeId,
                MarkAsDeleted = true
            };

            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(after.Mrid, true)).Returns(shadowTableSegment);

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var rollbackInvalidRouteSegment = (RollbackInvalidRouteNode)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                rollbackInvalidRouteSegment.Should().BeOfType(typeof(RollbackInvalidRouteNode));
            }
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

        [Fact]
        public async Task Create_ShouldReturnNamingInfoModified_OnUpdatedNamingInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            var after = new RouteNode
            {
                NamingInfo = new NamingInfo
                {
                    Description = "My description"
                }
            };

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var namingInfoUpdated = (RouteNodeNamingInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                namingInfoUpdated.RouteNode.Should().BeEquivalentTo(after);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnSafetyInfoUpdated_OnUpdatedSafetyInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteNode();
            var after = new RouteNode
            {
                SafetyInfo = new SafetyInfo
                {
                    Classification = "Clasification"
                }
            };

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var safetyInfoUpdated = (RouteNodeSafetyInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                safetyInfoUpdated.RouteNode.Should().BeEquivalentTo(after);
            }
        }
    }
}
