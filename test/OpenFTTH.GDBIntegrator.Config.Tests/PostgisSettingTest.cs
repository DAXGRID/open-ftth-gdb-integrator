using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

namespace OpenFTTH.GDBIntegrator.Config.Tests
{
    public class PostgisSettingTest
    {
        [Fact]
        public void PostgisSettingTest_ShouldInitalizeValues_OnConstruction()
        {
            var host = "192.13.2.3";
            var database = "OPEN_FTTH";
            var username = "postgres";
            var password = "1234";

            var postgisSetting = new PostgisSetting
            {
                Host = host,
                Database = database,
                Password = password,
                Username = username
            };

            using (new AssertionScope())
            {
                postgisSetting.Host.Should().BeEquivalentTo(host);
                postgisSetting.Database.Should().BeEquivalentTo(database);
                postgisSetting.Password.Should().BeEquivalentTo(password);
                postgisSetting.Username.Should().BeEquivalentTo(username);
            }
        }
    }
}
