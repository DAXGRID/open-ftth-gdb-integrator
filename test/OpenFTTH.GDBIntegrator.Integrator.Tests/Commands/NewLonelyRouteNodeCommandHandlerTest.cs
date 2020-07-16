using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class NewLonelyRouteNodeCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallProduceWithRouteNodeAddedOnce_OnBeingCalledWithRouteNode()
        {
            var logger = A.Fake<ILogger<NewLonelyRouteNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();
            var routeNode = A.Fake<RouteNode>();
            var newLonelyRouteNodeCommand = new NewLonelyRouteNodeCommand { RouteNode = routeNode };

            A.CallTo(() => kafkaSettings.Value)
                .Returns(new KafkaSetting { EventRouteNetworkTopicName = "event.route-network" });

            var newLonelyRouteNodeCommandHandler =
                new NewLonelyRouteNodeCommandHandler(logger, producer, kafkaSettings);

            await newLonelyRouteNodeCommandHandler
                     .Handle(newLonelyRouteNodeCommand, new CancellationToken());

            A.CallTo(() => producer
                     .Produce(kafkaSettings.Value.EventRouteNetworkTopicName, A<RouteNodeAdded>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentNullException_OnBeingCalledWithRouteNodeBeingNull()
        {
            var logger = A.Fake<ILogger<NewLonelyRouteNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();
            RouteNode routeNode = null;
            var newLonelyRouteNodeCommand = new NewLonelyRouteNodeCommand { RouteNode = routeNode };

            var newLonelyRouteNodeCommandHandler =
                new NewLonelyRouteNodeCommandHandler(logger, producer, kafkaSettings);

            Func<Task> act = async () =>
                {
                    await newLonelyRouteNodeCommandHandler
                        .Handle(newLonelyRouteNodeCommand, new CancellationToken());
                };

            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
