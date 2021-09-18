using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
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
        public async Task Create_ShouldThrowException_OnAfterBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            RouteSegment after = null;

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);

            Func<Task> act = async () => await factory.Create(before, after);

            await act.Should().ThrowExactlyAsync<Exception>("Invalid route segment update, before or after is null.");
        }

        [Fact]
        public async Task Create_ShouldThrowException_OnBeforeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteSegment before = null;
            var after = new RouteSegment();

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);

            Func<Task> act = async () => await factory.Create(before, after);

            await act.Should().ThrowExactlyAsync<Exception>("Invalid route segment update, before or after is null.");
        }

        [Fact]
        public async Task Create_ShouldThrowException_ReturnedGetRouteSegmentShadowTableBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteSegment before = null;
            var after = new RouteSegment();

            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(after.Mrid, true)).Returns<RouteSegment>(null);

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);

            Func<Task> act = async () => await factory.Create(before, after);

            await act.Should().ThrowExactlyAsync<Exception>(
                "Could not find {nameof(RouteSegment)} in shadowtable with id '{after.Mrid}'");
        }

        [Fact]
        public async Task Create_ShouldReturnDoNothing_OnShadowTableRouteSegmentBeingEqToAfter()
        {
            var routeSegmentId = Guid.NewGuid();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment
            {
                Mrid = routeSegmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Height = "10"
                }
            };
            var after = new RouteSegment
            {
                Mrid = routeSegmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Height = "10"
                }
            };
            var shadowTableSegment = new RouteSegment
            {
                Mrid = routeSegmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Height = "10"
                }
            };

            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(after.Mrid, true)).Returns(shadowTableSegment);

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var doNothingEvent = (DoNothing)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                doNothingEvent.Should().BeOfType(typeof(DoNothing));
            }
        }

        [Fact]
        public async Task Create_ShouldThrowException_OnBeforeBeingMarkedAsDeleted()
        {
            var routeSegmentId = Guid.NewGuid();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment
            {
                Mrid = routeSegmentId,
                MarkAsDeleted = true
            };
            var after = new RouteSegment
            {
                Mrid = routeSegmentId,
                RouteSegmentInfo = new RouteSegmentInfo
                {
                    Height = "10"
                }
            };
            var shadowTableSegment = new RouteSegment
            {
                Mrid = routeSegmentId,
                MarkAsDeleted = true
            };

            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(after.Mrid, true)).Returns(shadowTableSegment);

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);

            Func<Task> act = async () => await factory.Create(before, after);

            await act.Should().ThrowExactlyAsync<Exception>(
                $"Shadowtable {nameof(RouteSegment)} is marked to be deleted, info cannot be updated.");
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

        [Fact]
        public async Task Create_ShouldReturnSafetyInfoUpdated_OnUpdatedSafetyInfo()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var before = new RouteSegment();
            var after = new RouteSegment
            {
                SafetyInfo = new SafetyInfo
                {
                    Classification = "My test classification"
                }
            };

            var factory = new RouteSegmentInfoCommandFactory(geoDatabase);
            var result = await factory.Create(before, after);

            var infoUpdated = (RouteSegmentSafetyInfoUpdated)result.First();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                infoUpdated.RouteSegment.Should().BeEquivalentTo(after);
            }
        }
    }
}
