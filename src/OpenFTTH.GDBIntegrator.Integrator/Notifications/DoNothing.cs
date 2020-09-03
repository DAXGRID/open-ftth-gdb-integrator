using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class DoNothing : INotification
    {
        public string Message { get; }

        public DoNothing(string message)
        {
            Message = message;
        }
    }

    public class DoNothingHandler : INotificationHandler<DoNothing>
    {
        private readonly ILogger<DoNothingHandler> _logger;

        public DoNothingHandler(ILogger<DoNothingHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(DoNothing request, CancellationToken token)
        {
            _logger.LogInformation($"{request.Message}");
        }
    }
}
