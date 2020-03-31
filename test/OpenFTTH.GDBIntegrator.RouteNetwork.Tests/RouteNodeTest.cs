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
    }
}
