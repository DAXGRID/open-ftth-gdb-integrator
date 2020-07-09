using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Producer;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator
{
    public class Startup : IHostedService
    {
        private readonly IRouteSegmentSubscriber _subscriber;
        private readonly IProducer _producer;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Startup(IRouteSegmentSubscriber subscriber, IProducer producer, ILogger<Startup> logger, IHostApplicationLifetime applicationLifetime)
        {
            _subscriber = subscriber;
            _producer = producer;
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
            _logger.LogInformation("Starting GDB-Integrator");

            _logger.LogInformation("Starting to subscribe");
            _subscriber.Subscribe();

            _logger.LogInformation("Init producer");
            _producer.Init();
        }

        private void OnStopped()
        {
            _subscriber.Dispose();
            _producer.Dispose();
            _logger.LogInformation("Stopped");
        }
    }
}
