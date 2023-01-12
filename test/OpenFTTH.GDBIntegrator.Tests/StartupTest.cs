using FakeItEasy;
using FluentAssertions;
using FluentMigrator.Runner;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Subscriber;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Tests
{
    public class StartupTest
    {
        [Fact]
        public async Task StartAsync_ShouldNotThrow_OnBeingCalled()
        {
            var routeNetworkSubscriber = A.Fake<IRouteNetworkSubscriber>();
            var producer = A.Fake<IProducer>();
            var logger = A.Fake<ILogger<Startup>>();
            var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();
            var migrator = A.Fake<IMigrationRunner>();

            var startup = new Startup(routeNetworkSubscriber, producer, logger, hostApplicationLifetime, migrator);

            Func<Task> act = async () => { await startup.StartAsync(new CancellationToken()); };

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task StopAsync_ShouldNotThrow_OnBeingCalled()
        {
            var routeNetworkSubscriber = A.Fake<IRouteNetworkSubscriber>();
            var producer = A.Fake<IProducer>();
            var logger = A.Fake<ILogger<Startup>>();
            var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();
            var migrator = A.Fake<IMigrationRunner>();

            var startup = new Startup(routeNetworkSubscriber, producer, logger, hostApplicationLifetime, migrator);

            Func<Task> act = async () => { await startup.StopAsync(new CancellationToken()); };

            await act.Should().NotThrowAsync();
        }
    }
}
