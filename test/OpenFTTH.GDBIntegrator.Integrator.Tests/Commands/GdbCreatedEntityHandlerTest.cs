using Xunit;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class GdbCreatedEntityHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldJustExecute_OnBeingCalled()
        {
            var logger = A.Fake<ILogger<GdbCreatedEntityHandler>>();
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();

            var gdbCommandHandler = new GdbCreatedEntityHandler(logger, applicationSetting);

            var result = await gdbCommandHandler.Handle(new GdbCreatedEntity(), new CancellationToken());

            result.Should().NotBeNull();
        }
    }
}
