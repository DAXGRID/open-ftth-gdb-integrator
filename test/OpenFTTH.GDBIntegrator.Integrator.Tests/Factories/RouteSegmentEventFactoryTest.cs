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

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            Func<Task> act = async () => await factory.Create(null);

            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Create_ShouldReturnNull_OnRouteSegmentApplicationNameBeingEqualToSettingsApplicationName()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = new RouteSegment { ApplicationName = "GDB_INTEGRATOR" };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = await factory.Create(routeSegment);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_ShouldReturnInvalidRouteSegmentOperation_OnRouteSegmentLineStringBeingInvalid()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(false);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

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
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                result.RouteSegment.Should().Be(routeSegment);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingStartNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode>();

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                result.RouteSegment.Should().Be(routeSegment);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingEndNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                result.RouteSegment.Should().Be(routeSegment);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnNewRouteSegmentDigitizedByUser_OnIntersectingStartAndEndNodeCountBeingOne()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            A.CallTo(() => geoDatabase.GetIntersectingStartRouteNodes(routeSegment))
                .Returns(intersectingStartNodes);
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteNodes(routeSegment))
                .Returns(intersectingEndNodes);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<NewRouteSegmentDigitizedByUser>();
                result.RouteSegment.Should().Be(routeSegment);
                result.EventId.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Create_ShouldCallInsertRouteSegmentIntegrator_OnPassingMethodValidationChecks()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode> { A.Fake<RouteNode>() };
            var intersectingEndNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => applicationSettings.Value)
                .Returns(new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            A.CallTo(() => routeSegment.GetLineString()).Returns(lineString);
            A.CallTo(() => routeSegmentValidator.LineIsValid(lineString)).Returns(true);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = (NewRouteSegmentDigitizedByUser)(await factory.Create(routeSegment)).First();

            using (var scope = new AssertionScope())
            {
                A.CallTo(() => geoDatabase.InsertRouteSegmentIntegrator(routeSegment));
            }
        }

        [Fact]
        public async Task Create_ShouldContainStartCreateExistingRouteSegmentSplittedByUserForStartNode_On()
        {
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            var routeSegmentValidator = A.Fake<IRouteSegmentValidator>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeSegment = A.Fake<RouteSegment>();
            var startRouteNode = A.Fake<RouteNode>();
            var lineString = A.Fake<LineString>();
            var intersectingStartNodes = new List<RouteNode>();
            var intersectingEndNodes = new List<RouteNode>();
            var intersectingStartSegments = new List<RouteSegment> { A.Fake<RouteSegment>() };
            var intersectingEndSegments = new List<RouteSegment>();
           
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

            A.CallTo(() => routeSegment.FindStartNode()).Returns(startRouteNode);

            var factory = new RouteSegmentEventFactory(applicationSettings, routeSegmentValidator, geoDatabase);

            var result = await factory.Create(routeSegment);

            var existingRouteSegmentSplittedByUser = (ExistingRouteSegmentSplittedByUser)result.ToList()[0];
            var newRouteSegmentDigitizedByUser = (NewRouteSegmentDigitizedByUser)result.ToList()[1];

            using (var scope = new AssertionScope())
            {
                existingRouteSegmentSplittedByUser.EventId.Should().NotBeEmpty();
                existingRouteSegmentSplittedByUser.RouteNode.Should().Be(startRouteNode);
                existingRouteSegmentSplittedByUser.InsertRouteNode.Should().BeTrue();
                existingRouteSegmentSplittedByUser.RouteSegmentDigitizedByUser.Should().Be(routeSegment);

                newRouteSegmentDigitizedByUser.EventId.Should().NotBeEmpty();
                newRouteSegmentDigitizedByUser.RouteSegment.Should().Be(routeSegment);
            }
        }
    }
}
