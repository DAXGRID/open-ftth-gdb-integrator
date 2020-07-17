using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Producer;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class NewLonelyRouteSegmentHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallInsertTwoRouteNodes_OnBeingCalledWithRouteSegment()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewLonelyRouteSegmentHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewLonelyRouteSegmentHandler(geoDatabase, logger, producer, kafkaSetting);

            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();
            var routeSegment = A.Dummy<RouteSegment>();
            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);

            var command = new NewLonelyRouteSegment { RouteSegment = routeSegment };
            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustHaveHappenedOnceExactly();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowNullArgumentException_OnRouteSegmentBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewLonelyRouteSegmentHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();

            var command = new NewLonelyRouteSegment { RouteSegment = null };
            var commandHandler = new NewLonelyRouteSegmentHandler(geoDatabase, logger, producer, kafkaSetting);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_ShouldCallProduceRouteNodesForStartNodeAndEndNodeAndRouteSegment_OnBeingCalled()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewLonelyRouteSegmentHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSetting = A.Fake<IOptions<KafkaSetting>>();
            A.CallTo(() => kafkaSetting.Value).Returns(new KafkaSetting { EventRouteNetworkTopicName = "event.route-network" });

            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();
            var routeSegment = A.Dummy<RouteSegment>();
            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);

            var command = new NewLonelyRouteSegment { RouteSegment = routeSegment };
            var commandHandler = new NewLonelyRouteSegmentHandler(geoDatabase, logger, producer, kafkaSetting);

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => producer.Produce(kafkaSetting.Value.EventRouteNetworkTopicName, A<RouteNodeAdded>._))
                    .MustHaveHappenedTwiceExactly();
                A.CallTo(() => producer.Produce(kafkaSetting.Value.EventRouteNetworkTopicName, A<RouteSegmentAdded>._))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
