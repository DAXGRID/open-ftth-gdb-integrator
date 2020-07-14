using System;
using Xunit;
using FluentAssertions;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class NewRouteSegmentBetweenTwoExistingNodesCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallProduceWithRouteSegment_OnBeingCalledWithRouteSegment()
        {
            var logger = A.Fake<ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();
            A.CallTo(() => kafkaSetting.Value).Returns(new KafkaSetting { EventRouteNetworkTopicName = "event.route-network" });

            var commandHandler = new NewRouteSegmentBetweenTwoExistingNodesCommandHandler(logger, producer, kafkaSetting);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            var command = new NewRouteSegmentBetweenTwoExistingNodesCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            A.CallTo(() => producer.Produce(kafkaSetting.Value.EventRouteNetworkTopicName, A<RouteSegmentAdded>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldNeverCallProduceWithRouteNode_OnBeingCalledWithRouteSegment()
        {
            var logger = A.Fake<ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();
            A.CallTo(() => kafkaSetting.Value).Returns(new KafkaSetting { EventRouteNetworkTopicName = "event.route-network" });

            var commandHandler = new NewRouteSegmentBetweenTwoExistingNodesCommandHandler(logger, producer, kafkaSetting);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            var command = new NewRouteSegmentBetweenTwoExistingNodesCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            A.CallTo(() => producer.Produce(kafkaSetting.Value.EventRouteNetworkTopicName, A<RouteNodeAdded>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldThrowNullArgumentException_OnRouteSegmentBeingNull()
        {
            var logger = A.Fake<ILogger<NewRouteSegmentBetweenTwoExistingNodesCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();

            var command = new NewRouteSegmentBetweenTwoExistingNodesCommand { RouteSegment = null };
            var commandHandler = new NewRouteSegmentBetweenTwoExistingNodesCommandHandler(logger, producer, kafkaSetting);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
