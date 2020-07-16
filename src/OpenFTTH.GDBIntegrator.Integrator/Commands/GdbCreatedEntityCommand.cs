using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GdbCreatedEntityCommand : IRequest
    {
    }

    public class GdbCreatedEntityCommandHandler : IRequestHandler<GdbCreatedEntityCommand, Unit>
    {
        private readonly ILogger<GdbCreatedEntityCommandHandler> _logger;
        private readonly ApplicationSetting _applicationSetting;

        public GdbCreatedEntityCommandHandler(ILogger<GdbCreatedEntityCommandHandler> logger, IOptions<ApplicationSetting> applicationSetting)
        {
            _logger = logger;
            _applicationSetting = applicationSetting.Value;
        }

        public async Task<Unit> Handle(GdbCreatedEntityCommand command, CancellationToken token)
        {
            _logger.LogInformation(
                $"{DateTime.UtcNow.ToString("o")}: no action taken. Entity was created by {_applicationSetting.ApplicationName}");

            return await Task.FromResult(new Unit());
        }
    }
}
