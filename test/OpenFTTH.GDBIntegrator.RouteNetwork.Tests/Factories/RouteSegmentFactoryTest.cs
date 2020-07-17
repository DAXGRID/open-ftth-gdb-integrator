using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Test.Factories
{
    public class RouteSegmentFactoryTest
    {
        [Fact]
        public void Create_ShouldReturnListOfRouteSegmentsWithLineStrings_OnBeingPassedWktCollection()
        {
            var settings = A.Fake<IOptions<ApplicationSetting>>();
            var applicationName = "GDB_INTEGRATOR";
            A.CallTo(() => settings.Value).Returns(new ApplicationSetting { ApplicationName = applicationName });

            var routeSegmentFactory = new RouteSegmentFactory(settings);
            var result = routeSegmentFactory.Create("GEOMETRYCOLLECTION(LINESTRING(565750.295400957 6197728.17854604,565752.461433854 6197725.33562786),LINESTRING(565752.461433854 6197725.33562786,565754.520377439 6197722.6332644))");

            using (var scope = new AssertionScope())
            {
                result.Should().NotBeEmpty();
                result.Count.Should().Be(2);
                result.ForEach(x => x.Coord.Should().NotBeEmpty());
                result.ForEach(x => x.ApplicationName.Should().Be(applicationName));
            }
        }
    }
}
