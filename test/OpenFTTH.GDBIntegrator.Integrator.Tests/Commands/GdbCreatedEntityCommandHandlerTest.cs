using Xunit;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class GdbCreatedEntityCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldJustExecute_OnBeingCalled()
        {
            var logger = A.Fake<ILogger<GdbCreatedEntityCommandHandler>>();

            var gdbCommandHandler = new GdbCreatedEntityCommandHandler(logger);

            var result = await gdbCommandHandler.Handle(new GdbCreatedEntityCommand(), new CancellationToken());

            result.Should().NotBeNull();
        }
    }
}
