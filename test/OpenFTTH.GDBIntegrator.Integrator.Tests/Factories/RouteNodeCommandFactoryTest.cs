using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Integrator.Validate;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task CreateDigitizedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullRouteNode()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            Func<Task> act = async () => await factory.CreateDigitizedEvent(null);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnDoNothing_OnRouteNodeApplicationNameBeingSettingsApplicationName()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();

            A.CallTo(() => applicationSetting.Value).Returns(
                new ApplicationSetting { ApplicationName = "GDB_INTEGRATOR" });

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var routeNode = new RouteNode(Guid.Empty, null, Guid.Empty, String.Empty, "GDB_INTEGRATOR");
            var result = (DoNothing)((await factory.CreateDigitizedEvent(routeNode)).First());

            result.Should().BeOfType<DoNothing>();
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnRouteNodeAdded_OnIntersectingRouteSegmentsCountBeingZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(new List<RouteSegment>());

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (NewRouteNodeDigitized)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnExistingRouteSegmentSplitted_OnIntersectingRouteSegmentsCountBeingOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => routeNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = await factory.CreateDigitizedEvent(routeNode);
            var firstEvent = (ExistingRouteSegmentSplitted)result[0];

            using (new AssertionScope())
            {
                firstEvent.RouteNode.Should().BeEquivalentTo(routeNode);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnInvalidRouteNodeOperation_OnIntersectingRouteSegmentsCountBeingGreaterThanOne()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var routeSegments = new List<RouteSegment> { new RouteSegment(), new RouteSegment() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(routeNode))
                .Returns(routeSegments);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => routeNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (InvalidRouteNodeOperation)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
            }
        }

        [Fact]
        public async Task CreateDigitizedEvent_ShouldReturnInvalidRouteNodeOperation_OnIntersectingRouteNodeCountBeingGreaterThanZero()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNode = A.Fake<RouteNode>();
            var intersectingRouteNodes = new List<RouteNode> { A.Fake<RouteNode>() };

            A.CallTo(() => geoDatabase.GetIntersectingRouteNodes(routeNode))
                .Returns(intersectingRouteNodes);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => routeNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (InvalidRouteNodeOperation)((await factory.CreateDigitizedEvent(routeNode)).First());

            using (new AssertionScope())
            {
                result.RouteNode.Should().BeEquivalentTo(routeNode);
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeDeletedEvent_OnRouteNodeMarkAsDeletedSetAndNoIntersectingSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();
            var appSettings = new ApplicationSetting { Tolerance = 0.01 };

            var mrid = Guid.NewGuid();
            A.CallTo(() => beforeNode.MarkAsDeleted).Returns(false);
            A.CallTo(() => beforeNode.Mrid).Returns(mrid);
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);
            A.CallTo(() => afterNode.Mrid).Returns(mrid);
            A.CallTo(() => afterNode.GetPoint()).Returns(CreatePoint(565931.4446905176, 6197297.75114815));
            A.CallTo(() => shadowTableRouteNode.GetPoint()).Returns(CreatePoint(565930.4446905176, 6197297.75114815));
            A.CallTo(() => applicationSetting.Value).Returns(appSettings);
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);
            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode)).Returns(new List<RouteSegment> { });

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);
            var result = (RouteNodeDeleted)(await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            using (new AssertionScope())
            {
                result.RouteNode.Should().Be(afterNode);
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNodeOperation_OnRouteNodeMarkAsDeletedSetAndInsectsWithAnyRouteSegments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var shadowTableNode = A.Fake<RouteNode>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var validationService = A.Fake<IValidationService>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);
            A.CallTo(() => shadowTableNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => validationService.HasRelatedEquipment(afterNode.Mrid)).Returns(true);
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableNode);
            A.CallTo(() => geoDatabase.GetIntersectingStartRouteSegments(shadowTableNode)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });
            A.CallTo(() => geoDatabase.GetIntersectingEndRouteSegments(shadowTableNode)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });
            A.CallTo(() => afterNode.GetPoint()).Returns(CreatePoint(665931.4446905176, 7197297.75114815));
            A.CallTo(() => shadowTableNode.GetPoint()).Returns(CreatePoint(565931.4446905176, 6197297.75114815));

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);
            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);

            var expected = new RollbackInvalidRouteNode(shadowTableNode, "Is not a valid route node update.");
            using (var scope = new AssertionScope())
            {
                result.Should().HaveCount(1);
                result[0].Should().BeOfType(typeof(RollbackInvalidRouteNode)).And.BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNodeOperation_OnRouteNodeIntersectingWithOtherRouteNodes()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();
            var appSettings = new ApplicationSetting { Tolerance = 0.01 };

            A.CallTo(() => applicationSetting.Value).Returns(appSettings);
            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);
            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode)).Returns(new List<RouteSegment>());
            A.CallTo(() => afterNode.GetPoint()).Returns(CreatePoint(565931.4446905176, 6197297.75114815));
            A.CallTo(() => shadowTableRouteNode.GetPoint()).Returns(CreatePoint(565920.4446905176, 6197297.74114815));
            A.CallTo(() => afterNode.GetGeoJsonCoordinate(true)).Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate(true)).Returns("[565920.4446905176,6197297.74114815]");
            A.CallTo(() => geoDatabase.GetIntersectingRouteNodes(afterNode)).Returns(new List<RouteNode> { A.Fake<RouteNode>() });

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);
            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            var expected = new RollbackInvalidRouteNode(beforeNode, "Is not a valid route node update.");

            result.Should().BeOfType(typeof(RollbackInvalidRouteNode)).And.BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnRouteNodeMarkedAsDeletedAndCoordBeingTheSame()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var integratorRouteNode = A.Fake<RouteNode>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());

            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false))
                .Returns(integratorRouteNode);

            A.CallTo(() => afterNode.GetGeoJsonCoordinate(true))
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(true);

            A.CallTo(() => integratorRouteNode.GetGeoJsonCoordinate(true))
                .Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => integratorRouteNode.MarkAsDeleted).Returns(true);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            result.Should().BeOfType<DoNothing>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNode_OnIsModifiedDistanceLessThanTolerance()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();
            var appSettings = new ApplicationSetting { Tolerance = 0.01 };
            var point = CreatePoint(552428.7508312801, 6188868.185819111);

            A.CallTo(() => applicationSetting.Value).Returns(appSettings);
            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => afterNode.GetGeoJsonCoordinate(true)).Returns("[552428.7508312801, 6188868.185819111]");
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);
            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate(true)).Returns("[552428.7515896157,6188868.184787691]");
            A.CallTo(() => shadowTableRouteNode.GetPoint()).Returns(CreatePoint(552428.7515896157, 6188868.184787691));

            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            var expected = new RollbackInvalidRouteNode(shadowTableRouteNode, "Modified distance less than tolerance.");

            result.Should().BeOfType<RollbackInvalidRouteNode>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedNullArguments()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            RouteNode beforeNode = null;
            RouteNode afterNode = null;


            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedBeforeRouteNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            RouteNode beforeNode = null;
            var afterNode = A.Fake<RouteNode>();

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldThrowArgumentNullException_OnBeingPassedAfterNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            RouteNode afterNode = null;


            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            Func<Task> act = async () => await factory.CreateUpdatedEvent(beforeNode, afterNode);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnDoNothing_OnShadowTableRouteNodeBeingNull()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            RouteNode shadowTableRouteNode = null;

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            result.Should().BeOfType(typeof(DoNothing));
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRollbackInvalidRouteNode_OnIntersectingRouteSegment()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();
            var appSettingsValue = new ApplicationSetting { Tolerance = 0.01 };

            A.CallTo(() => applicationSetting.Value).Returns(appSettingsValue);
            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);
            A.CallTo(() => afterNode.GetGeoJsonCoordinate(true)).Returns("[665931.4446905176,7197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(false);
            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate(true)).Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => shadowTableRouteNode.MarkAsDeleted).Returns(false);
            A.CallTo(() => afterNode.GetPoint()).Returns(CreatePoint(665931.4446905176, 7197297.75114815));
            A.CallTo(() => shadowTableRouteNode.GetPoint()).Returns(CreatePoint(565931.4446905176, 6197297.75114815));
            A.CallTo(() => geoDatabase.GetIntersectingRouteSegments(afterNode)).Returns(new List<RouteSegment> { A.Fake<RouteSegment>() });

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = await factory.CreateUpdatedEvent(beforeNode, afterNode);
            var rollbackInvalidRouteNode = (RollbackInvalidRouteNode)result[0];

            var expected = new RollbackInvalidRouteNode(beforeNode, "Update to route node is invalid because it is insecting with route-segments.");

            using (var scope = new AssertionScope())
            {
                result.Should().HaveCount(1);
                rollbackInvalidRouteNode.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task CreateUpdatedEvent_ShouldReturnRouteNodeLocationChanged_OnRouteNodeChanged()
        {
            var applicationSetting = A.Fake<IOptions<ApplicationSetting>>();
            var geoDatabase = A.Fake<IGeoDatabase>();
            var beforeNode = A.Fake<RouteNode>();
            var afterNode = A.Fake<RouteNode>();
            var shadowTableRouteNode = A.Fake<RouteNode>();

            A.CallTo(() => afterNode.Mrid).Returns(Guid.NewGuid());
            A.CallTo(() => geoDatabase.GetRouteNodeShadowTable(afterNode.Mrid, false)).Returns(shadowTableRouteNode);
            A.CallTo(() => afterNode.GetPoint()).Returns(CreatePoint(665931.4446905176, 7197297.75114815));
            A.CallTo(() => shadowTableRouteNode.GetPoint()).Returns(CreatePoint(565931.4446905176, 6197297.75114815));
            A.CallTo(() => afterNode.GetGeoJsonCoordinate(true)).Returns("[665931.4446905176,7197297.75114815]");
            A.CallTo(() => afterNode.MarkAsDeleted).Returns(false);
            A.CallTo(() => shadowTableRouteNode.GetGeoJsonCoordinate(true)).Returns("[565931.4446905176,6197297.75114815]");
            A.CallTo(() => shadowTableRouteNode.MarkAsDeleted).Returns(false);

            var point = A.Fake<Point>();
            var routeNodeValidator = A.Fake<IRouteNodeValidator>();
            A.CallTo(() => afterNode.GetPoint()).Returns(point);
            A.CallTo(() => routeNodeValidator.PointIsValid(point)).Returns(true);

            var factory = new RouteNodeCommandFactory(applicationSetting, geoDatabase, routeNodeValidator);

            var result = (RouteNodeLocationChanged)(await factory.CreateUpdatedEvent(beforeNode, afterNode)).First();

            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType(typeof(RouteNodeLocationChanged));
                result.RouteNodeAfter.Should().Be(afterNode);
            }
        }

        private Point CreatePoint(double x, double y)
            => new Point(new Coordinate(x, y));
    }
}
