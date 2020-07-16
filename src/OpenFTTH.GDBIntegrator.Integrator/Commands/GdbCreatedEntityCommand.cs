using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands
{
    public class GdbCreatedEntityCommand : IRequest
    {
    }

    public class GdbCreatedEntityCommandHandler : IRequestHandler<GdbCreatedEntityCommand, Unit>
    {
        private readonly ILogger<GdbCreatedEntityCommandHandler> _logger;

        public GdbCreatedEntityCommandHandler(ILogger<GdbCreatedEntityCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(GdbCreatedEntityCommand command, CancellationToken token)
        {
            _logger.LogInformation($"{DateTime.UtcNow.ToString("o")}: Received message with enetity"
                                   + "that was crated by GdbIntegrator therefore no action is taken");

            return await Task.FromResult(new Unit());
        }
    }
}
