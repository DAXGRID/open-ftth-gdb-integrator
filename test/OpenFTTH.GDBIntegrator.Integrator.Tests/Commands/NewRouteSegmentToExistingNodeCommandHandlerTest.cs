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
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.EventMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Commands
{
    public class NewRouteSegmentToExistingNodeCommandHandlerTest
    {
        [Fact]
        public async Task Handle_ShouldCallInsertEndRouteNode_OnBeingCalledWithEndRouteNodeBeingNullAndStartNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);
            A.CallTo(() => endNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = null
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustNotHaveHappened();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldCallInsertStartRouteNode_OnBeingCalledWithStartRouteNodeBeingNullAndEndNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => startNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = null,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteNode(startNode)).MustHaveHappenedOnceExactly();
                A.CallTo(() => geoDatabase.InsertRouteNode(endNode)).MustNotHaveHappened();
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowNullArgumentException_OnRouteSegmentBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var command = new NewRouteSegmentToExistingNodeCommand { RouteSegment = null };
            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_OnBothStartAndEndNodeBeingNull()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = A.Fake<RouteSegment>(),
                StartRouteNode = null,
                EndRouteNode = null
            };

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);

            Func<Task> act = async () => { await commandHandler.Handle(command, new CancellationToken()); };
            await act.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        public async Task Handle_ShouldCallRouteNodeAddedCommandOnce_OnBeingCalledWithEndRouteNodeBeingNullAndStartNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);
            A.CallTo(() => endNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = null
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => producer.Produce(A<string>._, A<RouteNodeAdded>._)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldCallRouteNodeAddedOnce_OnBeingCalledWithStartRouteNodeBeingNullAndEndNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => startNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = null,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => producer.Produce(A<string>._, A<RouteNodeAdded>._)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldCallRouteSegmentAddedCommandOnce_OnBeingCalledWithEndRouteNodeBeingNullAndStartNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindEndNode()).Returns(endNode);
            A.CallTo(() => endNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = startNode,
                EndRouteNode = null
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => producer.Produce(A<string>._, A<RouteSegmentAdded>._)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Handle_ShouldCallRouteSegmentAddedCommandOnce_OnBeingCalledWithStartRouteNodeBeingNullAndEndNodeIsSet()
        {
            var geoDatabase = A.Fake<IGeoDatabase>();
            var logger = A.Fake<ILogger<NewRouteSegmentToExistingNodeCommandHandler>>();
            var producer = A.Fake<IProducer>();
            var kafkaSettings = A.Fake<IOptions<KafkaSetting>>();

            var commandHandler = new NewRouteSegmentToExistingNodeCommandHandler(geoDatabase, logger, producer, kafkaSettings);
            var routeSegment = A.Fake<RouteSegment>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegment.FindStartNode()).Returns(startNode);
            A.CallTo(() => startNode.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());
            A.CallTo(() => routeSegment.GetGeoJsonCoordinate()).Returns(A.Dummy<string>());

            var command = new NewRouteSegmentToExistingNodeCommand
            {
                RouteSegment = routeSegment,
                StartRouteNode = null,
                EndRouteNode = endNode
            };

            await commandHandler.Handle(command, new CancellationToken());

            using (new AssertionScope())
            {
                A.CallTo(() => producer.Produce(A<string>._, A<RouteSegmentAdded>._)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
