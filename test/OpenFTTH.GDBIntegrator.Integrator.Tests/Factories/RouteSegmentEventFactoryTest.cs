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
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedNullRouteSegment()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            Func<Task> act = async () => await factory.Create(null);

            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNull_OnRouteSegmentApplicationNameBeingEqualToSettingsApplicationName()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeFactory = A.Fake<IRouteNodeFactory>();
            var routeSegment = new RouteSegment { ApplicationName = "GDB_INTEGRATOR" };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase, routeNodeFactory);

            var result = await factory.Create(routeSegment);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_ShouldReturnInvalidRouteSegmentOperation_OnRouteSegmentLineStringBeingInvalid()
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

            var result = (InvalidRouteSegmentOperation)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<InvalidRouteSegmentOperation>();
                result.RouteSegment.Should().Be(routeSegment);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingStartAndEndRouteNodeBeingZero()
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

            var result = await factory.Create(routeSegment);

            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitizedByUser.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingStartNodeCountBeingOne()
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

            var result = await factory.Create(routeSegment);

            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitizedByUser.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingEndNodeCountBeingOne()
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

            var result = await factory.Create(routeSegment);

            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitizedByUser.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingStartAndEndNodeCountBeingOne()
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

            var result = await factory.Create(routeSegment);

            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[0];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(1);
                newRouteSegmentDigitizedByUser.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldCallInsertRouteSegmentIntegrator_OnPassingMethodValidationChecks()
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

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteSegmentIntegrator(routeSegment));
            }
        }

        [Fact]
        public async Task Create_ShouldContainStartCreateExistingRouteSegmentSplittedByUserForStartNode_OnStartSegmentCountBeingOneAndStartNodesCountBeingZero()
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

            var result = await factory.Create(routeSegment);

            var startInsertRouteNode = (InsertRouteNode)result.ToList()[0];
            var existingRouteSegmentSplittedByUser = (ExistingRouteSegmentSplittedByUser)result.ToList()[1];
            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[2];

            using (var scope = new AssertionScope())
            {
                startInsertRouteNode.RouteNode.Should().Be(startRouteNode);
                startInsertRouteNode.EventId.Should().NotBeEmpty();

                existingRouteSegmentSplittedByUser.EventId.Should().NotBeEmpty();
                existingRouteSegmentSplittedByUser.RouteNode.Should().Be(startRouteNode);
                existingRouteSegmentSplittedByUser.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task Create_ShouldContainEndCreateExistingRouteSegmentSplittedByUserForStartNode_OnEndSegmentCountBeingOneAndStartNodesCountBeingZero()
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

            var result = await factory.Create(routeSegment);

            var endInsertNode = (InsertRouteNode)result.ToList()[0];
            var existingRouteSegmentSplittedByUser = (ExistingRouteSegmentSplittedByUser)result.ToList()[1];
            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[2];

            using (var scope = new AssertionScope())
            {
                endInsertNode.RouteNode.Should().Be(endNode);
                endInsertNode.EventId.Should().NotBeEmpty();

                existingRouteSegmentSplittedByUser.EventId.Should().NotBeEmpty();
                existingRouteSegmentSplittedByUser.RouteNode.Should().Be(endNode);
                existingRouteSegmentSplittedByUser.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task Create_ShouldContainEndCreateExistingRouteSegmentSplittedByUserForStartNode_OnStartAndEndSegmentCountBeingOneAndStartAndEndNodesCountBeingZero()
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

            var result = await factory.Create(routeSegment);

            var startInsertNode = (InsertRouteNode)result.ToList()[0];
            var startExistingRouteSegmentSplittedByUser = (ExistingRouteSegmentSplittedByUser)result.ToList()[1];

            var endInsertNode = (InsertRouteNode)result.ToList()[2];
            var endExistingRouteSegmentSplittedByUser = (ExistingRouteSegmentSplittedByUser)result.ToList()[3];

            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[4];

            using (var scope = new AssertionScope())
            {
                startExistingRouteSegmentSplittedByUser.EventId.Should().NotBeEmpty();
                startExistingRouteSegmentSplittedByUser.RouteNode.Should().Be(startNode);
                startExistingRouteSegmentSplittedByUser.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                startInsertNode.RouteNode.Should().Be(startNode);
                startInsertNode.EventId.Should().NotBeEmpty();

                endExistingRouteSegmentSplittedByUser.EventId.Should().NotBeEmpty();
                endExistingRouteSegmentSplittedByUser.RouteNode.Should().Be(endNode);
                endExistingRouteSegmentSplittedByUser.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                endInsertNode.RouteNode.Should().Be(endNode);
                endInsertNode.EventId.Should().NotBeEmpty();

                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewSegmentAndSplittedRouteSegments_OnRouteSegmentIntersectingWithRouteNodesInGeometry()
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

            var result = (await factory.Create(routeSegment)).ToList();

            var newSegmentNotification = (NewRouteSegmentDigitizedByUser)result[0];
            var splittedRouteSegmentNotificationOne = (ExistingRouteSegmentSplittedByUser)result[1];
            var splittedRouteSegmentNotificationTwo = (ExistingRouteSegmentSplittedByUser)result[2];

            using (var scope = new AssertionScope())
            {
                result.Count().Should().Be(3);
                newSegmentNotification.RouteSegment.Should().BeEquivalentTo(routeSegment);
                newSegmentNotification.EventId.Should().NotBeEmpty();

                splittedRouteSegmentNotificationOne.EventId.Should().NotBeEmpty();
                splittedRouteSegmentNotificationOne.RouteNode.Should().BeEquivalentTo(allIntersectingRouteNodes[0]);
                splittedRouteSegmentNotificationOne.RouteSegmentDigitizedByUser.Should().BeNull();

                splittedRouteSegmentNotificationTwo.EventId.Should().NotBeEmpty();
                splittedRouteSegmentNotificationTwo.RouteNode.Should().BeEquivalentTo(allIntersectingRouteNodes[1]);
                splittedRouteSegmentNotificationTwo.RouteSegmentDigitizedByUser.Should().BeNull();
            }
        }
    }
}
