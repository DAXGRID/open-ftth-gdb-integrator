using OpenFTTH.GDBIntegrator.Subscriber;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator
{
    public class Startup : IHostedService
    {
        private readonly ISubscriber _subscriber;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Startup(ISubscriber subscriber, ILogger<Startup> logger, IHostApplicationLifetime applicationLifetime)
        {
            _subscriber = subscriber;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");

            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation("Started");

            _logger.LogInformation("Starting to subscribe");
            _subscriber.Subscribe();
        }

        private void OnStopped()
        {
            _logger.LogInformation("Stopped");
            _subscriber.Dispose();
        }
    }
}
