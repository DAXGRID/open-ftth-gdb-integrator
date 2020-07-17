using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

namespace OpenFTTH.GDBIntegrator.Config.Tests
{
    public class ApplicationSettingTest
    {
        [Fact]
        public void ApplicationSetting_ShouldInitalizeValues_OnConstruction()
        {
            var applicationName = "GDB_INTEGRATOR";
            var tolerance = 0.1;

            var applicationSetting = new ApplicationSetting
            {
                ApplicationName = applicationName,
                Tolerance = tolerance
            };

            using (new AssertionScope())
            {
                applicationName.Should().Be(applicationName);
                applicationSetting.Tolerance.Should().Be(tolerance);
            }
        }
    }
}
