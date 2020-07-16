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

            var applicationSetting = new ApplicationSetting
            {
                ApplicationName = applicationName,
            };

            using (new AssertionScope())
            {
                applicationName.Should().Be(applicationName);
            }
        }
    }
}
