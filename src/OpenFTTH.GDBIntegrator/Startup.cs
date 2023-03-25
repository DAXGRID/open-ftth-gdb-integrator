using FluentMigrator.Runner;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Subscriber;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator
{
    public class Startup : IHostedService
    {
        private readonly IRouteNetworkSubscriber _routeNetworkSubscriber;
        private readonly IProducer _producer;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IEventIdStore _eventIdStore;

        public Startup(
            IRouteNetworkSubscriber routeNetworkSubscriber,
            IProducer producer,
            ILogger<Startup> logger,
            IHostApplicationLifetime applicationLifetime,
            IMigrationRunner migrationRunner,
            IEventIdStore eventIdStore)
        {
            _routeNetworkSubscriber = routeNetworkSubscriber;
            _producer = producer;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _migrationRunner = migrationRunner;
            _eventIdStore = eventIdStore;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting GDB-Integrator.");

                _logger.LogInformation("Starting migration runner.");
                _migrationRunner.MigrateUp();

                _logger.LogInformation("Loading all existing event ids.");
                var eventIdsCount = await _eventIdStore
                    .LoadEventIds(cancellationToken)
                    .ConfigureAwait(false);
                _logger.LogInformation(
                    "Finished loading all existing event ids, {TotalCount}.",
                    eventIdsCount);

                _logger.LogInformation($"Starting {nameof(IRouteNetworkSubscriber)}");
                var subscriberTask = _routeNetworkSubscriber
                    .Subscribe(10, cancellationToken)
                    .ConfigureAwait(false);

                MarkAsReady();
                _logger.LogInformation($"Service is now in a healthy state.");

                await subscriberTask;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("{Exception}", ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping GDB-integrator");

            _routeNetworkSubscriber.Dispose();

            return Task.CompletedTask;
        }

        private void MarkAsReady()
        {
            File.Create("/tmp/healthy");
        }
    }
}
