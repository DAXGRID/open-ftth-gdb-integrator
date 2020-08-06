using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests
{
    public class RouteNodeTest
    {
        public static IEnumerable<object[]> GetRouteNodeData()
        {
            yield return new object[]
            {
                new Guid("fb1d3ffb-c424-4fd7-a1f1-a756167092c3"),
                new byte[] { 1, 2, 3, 2, 2 },
                new Guid("85d99983-b4b3-4ac8-8633-626b07ff89be"),
                "user_one",
                "qgis"
            };

            yield return new object[]
            {
                Guid.Empty,
                new byte[] { 1, 2, 3, 2, 2 },
                new Guid("85d99983-b4b3-4ac8-8633-626b07ff89be"),
                "user_one",
                "qgis"
            };

            yield return new object[]
            {
                Guid.Empty,
                new byte[] { 1, 2, 3, 2, 2 },
                Guid.Empty,
                null,
                "qgis"
            };

            yield return new object[]
            {
                Guid.Empty,
                new byte[] { 1, 2, 3, 2, 2 },
                Guid.Empty,
                null,
                null
            };

            yield return new object[]
            {
                Guid.Empty,
                null,
                Guid.Empty,
                null,
                null
            };
        }

        [Theory]
        [MemberData(nameof(RouteNodeTest.GetRouteNodeData))]
        public void RouteNode_ShouldSetInitialValues_OnConstruction(Guid mrid, byte[] coord, Guid workTaskMrId, string username, string applicationName)
        {
            var routeNode = new RouteNode(mrid, coord, workTaskMrId, username, applicationName);

            using (new AssertionScope())
            {
                routeNode.Mrid.Should().Be(mrid);
                routeNode.Coord.Should().BeEquivalentTo(coord);
                routeNode.WorkTaskMrid.Should().Be(workTaskMrId);
                routeNode.Username.Should().Be(username);
                routeNode.ApplicationName.Should().Be(applicationName);
            }
        }

        [Fact]
        public void RouteNode_ShouldSetInitialValuesOnEmptyConstructor_OnConstruction()
        {
            var routeNode = new RouteNode();

            using (new AssertionScope())
            {
                routeNode.Mrid.Should().BeEmpty();
                routeNode.Coord.Should().BeNull();
                routeNode.WorkTaskMrid.Should().BeEmpty();
                routeNode.Username.Should().BeNull();
                routeNode.ApplicationName.Should().BeNull();
            }
        }

        [Fact]
        public void RouteNode_ShouldSetInitialValuesOnSettingProperties_OnConstruction()
        {
            var applicationInfo = "Info";
            var applicationName = "GDB_INTEGRATOR";
            var coord = new byte[] { 1, 2, 3, 2, 2 };
            var nodeFunction = "SplicePoint";
            var nodeKind = "Connector";
            var nodeName = "ABC-13";
            var username = "myusername12";
            var mrid = Guid.NewGuid();
            var workTaskMrId = Guid.NewGuid();
            var markAsDeleted = false;

            var routeNode = new RouteNode
            {
                ApplicationInfo = applicationInfo,
                ApplicationName = applicationName,
                Coord = coord,
                NodeFunction = nodeFunction,
                NodeKind = nodeKind,
                NodeName = nodeName,
                Username = username,
                Mrid = mrid,
                WorkTaskMrid = workTaskMrId,
                MarkAsDeleted = markAsDeleted
            };

            using (new AssertionScope())
            {
                routeNode.ApplicationInfo.Should().Be(applicationInfo);
                routeNode.ApplicationName.Should().Be(applicationName);
                routeNode.Coord.Should().BeEquivalentTo(coord);
                routeNode.NodeFunction.Should().Be(nodeFunction);
                routeNode.NodeKind.Should().Be(nodeKind);
                routeNode.NodeName.Should().Be(nodeName);
                routeNode.Username.Should().Be(username);
                routeNode.Mrid.Should().Be(mrid);
                routeNode.WorkTaskMrid.Should().Be(workTaskMrId);
                routeNode.MarkAsDeleted.Should().Be(markAsDeleted);
            }
        }

        [Fact]
        public void GetGeoJsonCoordinate_ShouldReturnGeoJsonCoordinate_OnCalled()
        {
            var mrid = Guid.Empty;
            var coord = Convert.FromBase64String("AQEAAAC8ea7jVkUhQbHPEnAMpFdB");
            var workTaskMrId = Guid.Empty;
            var username = string.Empty;
            var applicationName = string.Empty;

            var routeNode = new RouteNode(mrid, coord, workTaskMrId, username, applicationName);

            var result = routeNode.GetGeoJsonCoordinate();

            result.Should().BeEquivalentTo("[565931.4446905176,6197297.75114815]");
        }
    }
}
