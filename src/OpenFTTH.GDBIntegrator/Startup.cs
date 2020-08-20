using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Producer;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentMigrator.Runner;

namespace OpenFTTH.GDBIntegrator
{
    public class Startup : IHostedService
    {
        private readonly IRouteNetworkSubscriber _routeNetworkSubscriber;
        private readonly IProducer _producer;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IMigrationRunner _migrationRunner;

        public Startup(
            IRouteNetworkSubscriber routeNetworkSubscriber,
            IProducer producer,
            ILogger<Startup> logger,
            IHostApplicationLifetime applicationLifetime,
            IMigrationRunner migrationRunner)
        {
            _routeNetworkSubscriber = routeNetworkSubscriber;
            _producer = producer;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _migrationRunner = migrationRunner;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");

            _migrationRunner.MigrateUp();

            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopped);

            MarkAsReady();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            return Task.CompletedTask;
        }

        private void MarkAsReady()
        {
            File.Create("/tmp/healthy");
        }

        private void OnStarted()
        {
            _logger.LogInformation("Starting GDB-Integrator");

            _logger.LogInformation($"Starting {nameof(IRouteNetworkSubscriber)}");
            _routeNetworkSubscriber.Subscribe();

            _logger.LogInformation("Init producer");
            _producer.Init();
        }

        private void OnStopped()
        {
            _routeNetworkSubscriber.Dispose();
            _producer.Dispose();
            _logger.LogInformation("Stopped");
        }
    }
}
