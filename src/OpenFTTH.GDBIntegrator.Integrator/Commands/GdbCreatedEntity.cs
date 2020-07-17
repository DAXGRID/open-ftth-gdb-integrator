using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GdbCreatedEntity : IRequest
    {
    }

    public class GdbCreatedEntityHandler : IRequestHandler<GdbCreatedEntity, Unit>
    {
        private readonly ILogger<GdbCreatedEntityHandler> _logger;
        private readonly ApplicationSetting _applicationSetting;

        public GdbCreatedEntityHandler(ILogger<GdbCreatedEntityHandler> logger, IOptions<ApplicationSetting> applicationSetting)
        {
            _logger = logger;
            _applicationSetting = applicationSetting.Value;
        }

        public async Task<Unit> Handle(GdbCreatedEntity command, CancellationToken token)
        {
            _logger.LogInformation(
                $"{DateTime.UtcNow.ToString("o")}: no action taken. Entity was created by {_applicationSetting.ApplicationName}");

            return await Task.FromResult(new Unit());
        }
    }
}
