using System;
using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Test.Factories
{
    public class RouteNodeFactoryTest
    {
        [Fact]
        public void Create_ShouldReturnRouteNode_OnBeingPassedPoint()
        {
            var settings = A.Fake<IOptions<ApplicationSetting>>();
            var applicationName = "GDB_INTEGRATOR";
            A.CallTo(() => settings.Value).Returns(new ApplicationSetting { ApplicationName = applicationName });

            var routeNodeFactory = new RouteNodeFactory(settings);

            var result = routeNodeFactory.Create(new Point(10.0, 10.0));

            using (var scope = new AssertionScope())
            {
                result.ApplicationName.Should().BeEquivalentTo(applicationName);
                result.Mrid.Should().NotBeEmpty();
                result.Username.Should().BeEquivalentTo(applicationName);
                result.Coord.Should().BeEquivalentTo(Convert.FromBase64String("AQEAAAAAAAAAAAAkQAAAAAAAACRA"));
            }
        }
    }
}
