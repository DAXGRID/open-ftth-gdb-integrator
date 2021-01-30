using System;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class ModifiedEventFactoryTest
    {
        [Fact]
        public void CreateRouteSegmentInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            modifiedEventFactory
                .Invoking(x => x.CreateRouteSegmentInfoModified(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateRouteSegmentInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var segmentId = Guid.NewGuid();

            var routeSegment = new RouteSegment
            {
                Mrid = segmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Width = "10cm",
                    Height = "",
                    Kind = (RouteSegmentKindEnum?)RouteNodeKindEnum.BuildingAccessPoint
                },
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
            };

            var result = modifiedEventFactory.CreateRouteSegmentInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("RouteSegmentInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.SegmentId.Should().Be(segmentId);
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.RouteSegmentInfo.Width.Should().BeEquivalentTo(routeSegment.RouteSegmentInfo.Width);
                result.RouteSegmentInfo.Height.Should().BeEquivalentTo(null);
                result.RouteSegmentInfo.Kind.Should().BeEquivalentTo(routeSegment.RouteSegmentInfo.Kind);
            }
        }

        [Fact]
        public void CreateRouteNodeInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            modifiedEventFactory.Invoking(x => x.CreateRouteNodeInfoModified(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateRouteNodeInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var routeNodeId = Guid.NewGuid();

            var routeNode = new RouteNode
            {
                Mrid = routeNodeId,
                ApplicationName = "GDB-integrator",
                RouteNodeInfo = new RouteNodeInfo
                {
                    Function = RouteNodeFunctionEnum.FlexPoint,
                    Kind = RouteNodeKindEnum.BuildingAccessPoint,
                },
                ApplicationInfo = "Application info",
            };

            var result = modifiedEventFactory.CreateRouteNodeInfoModified(routeNode);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("RouteNodeInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.NodeId.Should().Be(routeNodeId);
                result.ApplicationName.Should().Be(routeNode.ApplicationName);
                result.ApplicationInfo.Should().Be(routeNode.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.RouteNodeInfo.Should().BeEquivalentTo(routeNode.RouteNodeInfo);
            }
        }

        [Fact]
        public void CreateLifeCycleInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteNode routeNode = null;
            modifiedEventFactory.Invoking(x => x.CreateLifeCycleInfoModified(routeNode)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateLifecycleInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeNode = new RouteNode
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                LifeCycleInfo = new LifecycleInfo
                {
                    DeploymentState = DeploymentStateEnum.Installed,
                    InstallationDate = DateTime.UtcNow,
                    RemovalDate = DateTime.UtcNow.AddDays(1)
                },
            };

            var result = modifiedEventFactory.CreateLifeCycleInfoModified(routeNode);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("LifecycleInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeNode.Mrid);
                result.AggregateType.Should().Be("RouteNode");
                result.ApplicationName.Should().Be(routeNode.ApplicationName);
                result.ApplicationInfo.Should().Be(routeNode.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.LifecycleInfo.Should().BeEquivalentTo(routeNode.LifeCycleInfo);
            }
        }

        [Fact]
        public void CreateLifeCycleInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteSegment routeSegment = null;
            modifiedEventFactory
                .Invoking(x => x.CreateLifeCycleInfoModified(routeSegment)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateLifecycleInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeSegment = new RouteSegment
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                LifeCycleInfo = new LifecycleInfo
                {
                    DeploymentState = DeploymentStateEnum.Installed,
                    InstallationDate = DateTime.UtcNow,
                    RemovalDate = DateTime.UtcNow.AddDays(1)
                },
            };

            var result = modifiedEventFactory.CreateLifeCycleInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("LifecycleInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeSegment.Mrid);
                result.AggregateType.Should().Be("RouteSegment");
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.LifecycleInfo.Should().BeEquivalentTo(routeSegment.LifeCycleInfo);
            }
        }

        [Fact]
        public void CreateMappingInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteNode routeNode = null;
            modifiedEventFactory
                .Invoking(x => x.CreateMappingInfoModified(routeNode)).Should().Throw<ArgumentNullException>();
        }


        [Fact]
        public void CreateMappingInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeNode = new RouteNode
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                MappingInfo = new MappingInfo
                {
                    HorizontalAccuracy = "10",
                    Method = MappingMethodEnum.Drafting,
                    SourceInfo = "Some source info",
                    SurveyDate = DateTime.UtcNow,
                    VerticalAccuracy = "Vertical accuracy"
                }
            };

            var result = modifiedEventFactory.CreateMappingInfoModified(routeNode);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("MappingInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeNode.Mrid);
                result.AggregateType.Should().Be("RouteNode");
                result.ApplicationName.Should().Be(routeNode.ApplicationName);
                result.ApplicationInfo.Should().Be(routeNode.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.MappingInfo.Should().BeEquivalentTo(routeNode.MappingInfo);
            }
        }

        [Fact]
        public void CreateMappingInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteSegment routeSegment = null;
            modifiedEventFactory
                .Invoking(x => x.CreateMappingInfoModified(routeSegment)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateMappingInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeSegment = new RouteSegment
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                MappingInfo = new MappingInfo
                {
                    HorizontalAccuracy = "10",
                    Method = MappingMethodEnum.Drafting,
                    SourceInfo = "Some source info",
                    SurveyDate = DateTime.UtcNow,
                    VerticalAccuracy = "Vertical accuracy"
                }
            };

            var result = modifiedEventFactory.CreateMappingInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("MappingInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeSegment.Mrid);
                result.AggregateType.Should().Be("RouteSegment");
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.MappingInfo.Should().BeEquivalentTo(routeSegment.MappingInfo);
            }
        }

        [Fact]
        public void CreateSafetyInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteSegment routeSegment = null;
            modifiedEventFactory
                .Invoking(x => x.CreateSafetyInfoModified(routeSegment)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateSafetyInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeSegment = new RouteSegment
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                SafetyInfo = new SafetyInfo
                {
                    Classification = "My classification",
                    Remark = "My remark"
                }
            };

            var result = modifiedEventFactory.CreateSafetyInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("SafetyInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeSegment.Mrid);
                result.AggregateType.Should().Be("RouteSegment");
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.SafetyInfo.Should().BeEquivalentTo(routeSegment.SafetyInfo);
            }
        }

        [Fact]
        public void CreateSafetyInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteNode routeNode = null;
            modifiedEventFactory
                .Invoking(x => x.CreateSafetyInfoModified(routeNode)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateSafetyInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeNode = new RouteNode
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                SafetyInfo = new SafetyInfo
                {
                    Classification = "My classification",
                    Remark = "My remark"
                }
            };

            var result = modifiedEventFactory.CreateSafetyInfoModified(routeNode);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("SafetyInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeNode.Mrid);
                result.AggregateType.Should().Be("RouteNode");
                result.ApplicationName.Should().Be(routeNode.ApplicationName);
                result.ApplicationInfo.Should().Be(routeNode.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.SafetyInfo.Should().BeEquivalentTo(routeNode.SafetyInfo);
            }
        }

        [Fact]
        public void CreateNamingInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteSegment routeSegment = null;
            modifiedEventFactory
                .Invoking(x => x.CreateNamingInfoModified(routeSegment)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateNamingInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteSegment()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeSegment = new RouteSegment
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                NamingInfo = new NamingInfo
                {
                    Description = "My description",
                    Name = "My name"
                }
            };

            var result = modifiedEventFactory.CreateNamingInfoModified(routeSegment);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("NamingInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeSegment.Mrid);
                result.AggregateType.Should().Be("RouteSegment");
                result.ApplicationName.Should().Be(routeSegment.ApplicationName);
                result.ApplicationInfo.Should().Be(routeSegment.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.NamingInfo.Should().BeEquivalentTo(routeSegment.NamingInfo);
            }
        }

        [Fact]
        public void CreateNamingInfoModified_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();
            RouteNode routeNode = null;
            modifiedEventFactory
                .Invoking(x => x.CreateNamingInfoModified(routeNode)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateNamingInfoModified_ShouldReturnEvent_OnBeingPassedValidRouteNode()
        {
            var modifiedEventFactory = new ModifiedEventFactory();

            var nodeId = Guid.NewGuid();
            var routeNode = new RouteNode
            {
                Mrid = nodeId,
                ApplicationName = "GDB-integrator",
                ApplicationInfo = "Application info",
                NamingInfo = new NamingInfo
                {
                    Description = "My description",
                    Name = "My name"
                }
            };

            var result = modifiedEventFactory.CreateNamingInfoModified(routeNode);

            using (var scope = new AssertionScope())
            {
                result.EventType.Should().Be("NamingInfoModified");
                result.EventId.Should().NotBeEmpty();
                result.AggregateId.Should().Be(routeNode.Mrid);
                result.AggregateType.Should().Be("RouteNode");
                result.ApplicationName.Should().Be(routeNode.ApplicationName);
                result.ApplicationInfo.Should().Be(routeNode.ApplicationInfo);
                result.EventTimestamp.Should().NotBe(new DateTime());
                result.NamingInfo.Should().BeEquivalentTo(routeNode.NamingInfo);
            }
        }
    }
}
