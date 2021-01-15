using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using System;
using FluentAssertions;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeInfoCommandFactoryTest
    {
        [Fact]
        public async Task CreateNodeModifiedInfoEvents_ShouldThrowException_OnNodeAfterOrBeforeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();

            var factory = new RouteNodeInfoCommandFactory(geoDatabase);

            // Both being null
            Func<Task> actOne = async () =>
                await factory.CreateNodeModifiedInfoEvents(null, null);
            await actOne.Should().ThrowExactlyAsync<ArgumentNullException>();

            // Before being null
            Func<Task> actTwo = async () =>
                await factory.CreateNodeModifiedInfoEvents(null, new RouteNode());
            await actTwo.Should().ThrowExactlyAsync<ArgumentNullException>();

            // After being null
            Func<Task> actThree = async () =>
                await factory.CreateNodeModifiedInfoEvents(new RouteNode(), null);
            await actThree.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
