using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteSegmentEventFactoryTest
    {
        [Fact]
        public async Task CreateDigitizedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            Func<Task> act = async () => await factory.CreateDigitizedEvent(null);

            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNull_OnRouteSegmentApplicationNameBeingEqualToSettingsApplicationName()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = new RouteSegment { ApplicationName = "GDB_INTEGRATOR" };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnInvalidRouteSegmentOperation_OnRouteSegmentLineStringBeingInvalid()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(false);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (InvalidRouteSegmentOperation)(await factory.CreateDigitizedEvent(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<InvalidRouteSegmentOperation>();
                result.RouteSegment.Should().Be(routeSegment);
                result.CmdId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNewRouteSegmentDigitized_OnIntersectingStartAndEndRouteNodeBeingZero()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();
            var allIntersectingRouteNodes = new List<RouteNode>();

            A.CallTo(() => geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment))
                .Returns(allIntersectingRouteNodes);

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitized.Should().BeOfType<NewRouteSegmentDigitized>();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNewRouteSegmentDigitized_OnIntersectingStartNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode>();
            var allIntersectingRouteNodes = new List<RouteNode>();

            A.CallTo(() => geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment))
                .Returns(allIntersectingRouteNodes);

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitized.Should().BeOfType<NewRouteSegmentDigitized>();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNewRouteSegmentDigitized_OnIntersectingEndNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var allIntersectingRouteNodes = new List<RouteNode>();

            A.CallTo(() => geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment))
                .Returns(allIntersectingRouteNodes);

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitized.Should().BeOfType<NewRouteSegmentDigitized>();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNewRouteSegmentDigitized_OnIntersectingStartAndEndNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var allIntersectingRouteNodes = new List<RouteNode>();

            A.CallTo(() => geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment))
                .Returns(allIntersectingRouteNodes);

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitized.Should().BeOfType<NewRouteSegmentDigitized>();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldCallInsertRouteSegmentIntegrator_OnPassingMethodValidationChecks()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (NewRouteSegmentDigitized)(await factory.CreateDigitizedEvent(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteSegmentShadowTable(routeSegment));
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldContainStartCreateExistingRouteSegmentSplittedForStartNode_OnStartSegmentCountBeingOneAndStartNodesCountBeingZero()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var startRouteNode = A.Fake<RouteNode>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();
            var intersectingStartSegments = new List<RouteSegment> { A.Fake<RouteSegment>() };
            var intersectingEndSegments = new List<RouteSegment>();
            var startPoint = A.Fake<Point>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(routeSegment))
                .Returns(intersectingStartSegments);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(routeSegment))
                .Returns(intersectingEndSegments);

            A.CallTo(() => routeSegment.FindStartPoint()).Returns(startPoint);
            A.CallTo(() => routeNodeFactory.Create(startPoint)).Returns(startRouteNode);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var startInsertRouteNode = (NewRouteNodeDigitized)result.ToList()[0];
            var existingRouteSegmentSplitted = (ExistingRouteSegmentSplitted)result.ToList()[1];
            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[2];

            using (var scope = new AssertionScope())
            {
                startInsertRouteNode.RouteNode.Should().Be(startRouteNode);
                startInsertRouteNode.CmdId.Should().NotBeEmpty();

                existingRouteSegmentSplitted.CmdId.Should().NotBeEmpty();
                existingRouteSegmentSplitted.RouteNode.Should().Be(startRouteNode);
                existingRouteSegmentSplitted.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldContainEndCreateExistingRouteSegmentSplittedForStartNode_OnEndSegmentCountBeingOneAndStartNodesCountBeingZero()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var endNode = A.Fake<RouteNode>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();
            var intersectingStartSegments = new List<RouteSegment>();
            var intersectingEndSegments = new List<RouteSegment> { A.Fake<RouteSegment>() };
            var endPoint = A.Fake<Point>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(routeSegment))
                .Returns(intersectingStartSegments);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(routeSegment))
                .Returns(intersectingEndSegments);

            A.CallTo(() => routeSegment.FindEndPoint()).Returns(endPoint);
            A.CallTo(() => routeNodeFactory.Create(endPoint)).Returns(endNode);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var endInsertNode = (NewRouteNodeDigitized)result.ToList()[0];
            var existingRouteSegmentSplitted = (ExistingRouteSegmentSplitted)result.ToList()[1];
            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[2];

            using (var scope = new AssertionScope())
            {
                endInsertNode.RouteNode.Should().Be(endNode);
                endInsertNode.CmdId.Should().NotBeEmpty();

                existingRouteSegmentSplitted.CmdId.Should().NotBeEmpty();
                existingRouteSegmentSplitted.RouteNode.Should().Be(endNode);
                existingRouteSegmentSplitted.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldContainEndCreateExistingRouteSegmentSplittedForStartNode_OnStartAndEndSegmentCountBeingOneAndStartAndEndNodesCountBeingZero()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var startNode = A.Fake<RouteNode>();
            var endNode = A.Fake<RouteNode>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();
            var intersectingStartSegments = new List<RouteSegment> { A.Fake<RouteSegment>() };
            var intersectingEndSegments = new List<RouteSegment> { A.Fake<RouteSegment>() };
            var startPoint = A.Fake<Point>();
            var endPoint = A.Fake<Point>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(routeSegment))
                .Returns(intersectingStartSegments);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(routeSegment))
                .Returns(intersectingEndSegments);

            A.CallTo(() => routeSegment.FindStartPoint()).Returns(startPoint);
            A.CallTo(() => routeSegment.FindEndPoint()).Returns(endPoint);

            A.CallTo(() => routeNodeFactory.Create(startPoint)).Returns(startNode);
            A.CallTo(() => routeNodeFactory.Create(endPoint)).Returns(endNode);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.CreateDigitizedEvent(routeSegment);

            var startInsertNode = (NewRouteNodeDigitized)result.ToList()[0];
            var startExistingRouteSegmentSplitted = (ExistingRouteSegmentSplitted)result.ToList()[1];

            var endInsertNode = (NewRouteNodeDigitized)result.ToList()[2];
            var endExistingRouteSegmentSplitted = (ExistingRouteSegmentSplitted)result.ToList()[3];

            var newRouteSegmentDigitized = (NewRouteSegmentDigitized)result.ToList()[4];

            using (var scope = new AssertionScope())
            {
                startExistingRouteSegmentSplitted.CmdId.Should().NotBeEmpty();
                startExistingRouteSegmentSplitted.RouteNode.Should().Be(startNode);
                startExistingRouteSegmentSplitted.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                startInsertNode.RouteNode.Should().Be(startNode);
                startInsertNode.CmdId.Should().NotBeEmpty();

                endExistingRouteSegmentSplitted.CmdId.Should().NotBeEmpty();
                endExistingRouteSegmentSplitted.RouteNode.Should().Be(endNode);
                endExistingRouteSegmentSplitted.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                endInsertNode.RouteNode.Should().Be(endNode);
                endInsertNode.CmdId.Should().NotBeEmpty();

                newRouteSegmentDigitized.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitized.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnNewSegmentAndSplittedRouteSegments_OnRouteSegmentIntersectingWithRouteNodesInGeometry()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var allIntersectingRouteNodes = new List<RouteNode>
            {
                A.Fake<RouteNode>(),
                A.Fake<RouteNode>()
            };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetAllIntersectingRouteNodesNotIncludingEdges(routeSegment))
                .Returns(allIntersectingRouteNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (await factory.CreateDigitizedEvent(routeSegment)).ToList();

            var newSegmentNotification = (NewRouteSegmentDigitized)result[0];
            var splittedRouteSegmentNotificationOne = (ExistingRouteSegmentSplitted)result[1];
            var splittedRouteSegmentNotificationTwo = (ExistingRouteSegmentSplitted)result[2];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(3);
                newSegmentNotification.RouteSegment.Should().BeEquivalentTo(routeSegment);
                newSegmentNotification.EventId.Should().NotBeEmpty();

                splittedRouteSegmentNotificationOne.CmdId.Should().NotBeEmpty();
                splittedRouteSegmentNotificationOne.RouteNode.Should().BeEquivalentTo(allIntersectingRouteNodes[0]);
                splittedRouteSegmentNotificationOne.RouteSegmentDigitizedByUser.Should().BeNull();

                splittedRouteSegmentNotificationTwo.CmdId.Should().NotBeEmpty();
                splittedRouteSegmentNotificationTwo.RouteNode.Should().BeEquivalentTo(allIntersectingRouteNodes[1]);
                splittedRouteSegmentNotificationTwo.RouteSegmentDigitizedByUser.Should().BeNull();
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteSegmentMarkedForDeletion_OnRouteSegmentAfterMarkedToBeDeletedIsTrue()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();

            A.CallTo(() => routeSegmentAfter.GetLineString()).Returns(A.Fake<LineString>());
            A.CallTo(() => routeSegmentValidator.LineIsValid(routeSegmentAfter.GetLineString())).Returns(true);
            A.CallTo(() => routeSegmentAfter.MarkAsDeleted).Returns(true);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (RouteSegmentDeleted)(await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).First();

            using (var scope = new AssertionScope())
            {
                result.CmdId.Should().NotBeEmpty();
                result.RouteSegment.Should().Be(routeSegmentAfter);
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnRouteSegmentAfterAndShadowTableBeforeBeingEqual()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();
            var routeSegmentShadowTable = A.Fake<RouteSegment>();

            A.CallTo(() => routeSegmentAfter.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => routeSegmentShadowTable.Mrid).Returns(routeSegmentAfter.Mrid);
            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(routeSegmentAfter.Mrid)).Returns(routeSegmentShadowTable);
            A.CallTo(() => routeSegmentAfter.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentShadowTable.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentShadowTable.MarkAsDeleted).Returns(false);
            A.CallTo(() => routeSegmentAfter.MarkAsDeleted).Returns(false);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (DoNothing)(await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).First();

            result.Should().BeOfType(typeof(DoNothing));
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackSegment_OnGeometryBeingInvalid()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();
            var routeSegmentShadowTable = A.Fake<RouteSegment>();

            A.CallTo(() => routeSegmentAfter.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(routeSegmentAfter.Mrid)).Returns(routeSegmentShadowTable);
            A.CallTo(() => routeSegmentShadowTable.Mrid).Returns(routeSegmentAfter.Mrid);
            A.CallTo(() => routeSegmentAfter.GetLineString()).Returns(A.Fake<LineString>());

            A.CallTo(() => routeSegmentAfter.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentShadowTable.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 378238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentAfter.MarkAsDeleted).Returns(false);
            A.CallTo(() => routeSegmentShadowTable.MarkAsDeleted).Returns(false);

            A.CallTo(() => routeSegmentValidator.LineIsValid(routeSegmentAfter.GetLineString())).Returns(false);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).First();

            var expected = new RollbackInvalidRouteSegment(routeSegmentBefore);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnGetRouteSegmentFromShadowTableBeingNull()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();
            RouteSegment routeSegmentShadowTable = null;

            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(routeSegmentAfter.Mrid)).Returns(routeSegmentShadowTable);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (DoNothing)(await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).First();

            result.Should().BeOfType(typeof(DoNothing));
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnTwoSplitEventsAndConnectivityChanged_OnRouteSegmentBeingValidAndIntersectsWithOtherRouteSegmentNotEdge()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();
            var routeSegmentShadowTable = A.Fake<RouteSegment>();

            A.CallTo(() => routeSegmentAfter.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(routeSegmentAfter.Mrid)).Returns(routeSegmentShadowTable);
            A.CallTo(() => routeSegmentShadowTable.Mrid).Returns(routeSegmentAfter.Mrid);
            A.CallTo(() => routeSegmentAfter.GetLineString()).Returns(A.Fake<LineString>());

            A.CallTo(() => routeSegmentAfter.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentShadowTable.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 378238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentAfter.MarkAsDeleted).Returns(false);
            A.CallTo(() => routeSegmentShadowTable.MarkAsDeleted).Returns(false);

            A.CallTo(() => routeSegmentValidator.LineIsValid(routeSegmentAfter.GetLineString())).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(routeSegmentAfter)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(routeSegmentAfter)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });
            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegmentAfter)).Returns(new List<RouteNode>());
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegmentAfter)).Returns(new List<RouteNode>());

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).ToList();

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(5);
                result[0].Should().BeOfType(typeof(NewRouteNodeDigitized));
                result[1].Should().BeOfType(typeof(ExistingRouteSegmentSplitted));
                result[2].Should().BeOfType(typeof(NewRouteNodeDigitized));
                result[3].Should().BeOfType(typeof(ExistingRouteSegmentSplitted));
                result[4].Should().BeOfType(typeof(RouteSegmentConnectivityChanged));
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteSegmentLocationChanged_OnGeometryChangedAndNodesBeingSameAsBeforeUpdate()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegmentBefore = A.Fake<RouteSegment>();
            var routeSegmentAfter = A.Fake<RouteSegment>();
            var routeSegmentShadowTable = A.Fake<RouteSegment>();
            var currentStartNode = A.Fake<RouteNode>();
            var currentEndNode = A.Fake<RouteNode>();
            var previousStartNode = A.Fake<RouteNode>();
            var previousEndNode = A.Fake<RouteNode>();

            A.CallTo(() => routeSegmentAfter.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteSegmentShadowTable(routeSegmentAfter.Mrid)).Returns(routeSegmentShadowTable);
            A.CallTo(() => routeSegmentShadowTable.Mrid).Returns(routeSegmentAfter.Mrid);
            A.CallTo(() => routeSegmentAfter.GetLineString()).Returns(A.Fake<LineString>());

            A.CallTo(() => routeSegmentAfter.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentShadowTable.GetGeoJsonCoordinate()).Returns("LINESTRING(578223.64355838 6179284.23759438, 378238.4182511 6179279.78494725)");
            A.CallTo(() => routeSegmentAfter.MarkAsDeleted).Returns(false);
            A.CallTo(() => routeSegmentShadowTable.MarkAsDeleted).Returns(false);

            A.CallTo(() => routeSegmentValidator.LineIsValid(routeSegmentAfter.GetLineString())).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(routeSegmentAfter)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(routeSegmentAfter)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });           

            // Important for this test
            var startNodeMrid = Guid.NewGuid();
            var endNodeMrId = Guid.NewGuid();

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegmentAfter)).Returns(new List<RouteNode> { currentStartNode });
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegmentAfter)).Returns(new List<RouteNode> { currentEndNode });
            A.CallTo(() => currentStartNode.Mrid).Returns(startNodeMrid);
            A.CallTo(() => currentEndNode.Mrid).Returns(endNodeMrId);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegmentBefore.Coord)).Returns(new List<RouteNode> { previousStartNode });
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegmentBefore.Coord)).Returns(new List<RouteNode> { previousEndNode });
            A.CallTo(() => previousStartNode.Mrid).Returns(startNodeMrid);
            A.CallTo(() => previousEndNode.Mrid).Returns(endNodeMrId);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = (RouteSegmentLocationChanged)(await factory.CreateUpdatedEvent(routeSegmentBefore, routeSegmentAfter)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType(typeof(RouteSegmentLocationChanged));
                result.CmdId.Should().NotBeEmpty();
                result.RouteSegment.Should().Be(routeSegmentAfter);
            }
        }
    }
}
