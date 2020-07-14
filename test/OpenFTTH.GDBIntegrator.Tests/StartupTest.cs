using OpenFTTH.GDBIntegrator;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using Xunit;
using OpenFTTH.GDBIntegrator.Subscriber;
using OpenFTTH.GDBIntegrator.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace OpenFTTH.GDBIntegrator.Tests
{
    public class StartupTest
    {
        [Fact]
        public async Task StartAsync_ShouldNotThrow_OnBeingCalled()
        {
            var routeSegmentSubscriber = A.Fake<IRouteSegmentSubscriber>();
            var routeNodeSubscriber = A.Fake<IRouteNodeSubscriber>();
            var producer = A.Fake<IProducer>();
            var logger = A.Fake<ILogger<Startup>>();
            var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();

            var startup = new Startup(routeSegmentSubscriber, routeNodeSubscriber, producer, logger, hostApplicationLifetime);

            Func<Task> act = async () => { await startup.StartAsync(new CancellationToken()); };

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task StopAsync_ShouldNotThrow_OnBeingCalled()
        {
            var routeSegmentSubscriber = A.Fake<IRouteSegmentSubscriber>();
            var routeNodeSubscriber = A.Fake<IRouteNodeSubscriber>();
            var producer = A.Fake<IProducer>();
            var logger = A.Fake<ILogger<Startup>>();
            var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();

            var startup = new Startup(routeSegmentSubscriber, routeNodeSubscriber, producer, logger, hostApplicationLifetime);

            Func<Task> act = async () => { await startup.StopAsync(new CancellationToken()); };

            await act.Should().NotThrowAsync();
        }
    }
}
