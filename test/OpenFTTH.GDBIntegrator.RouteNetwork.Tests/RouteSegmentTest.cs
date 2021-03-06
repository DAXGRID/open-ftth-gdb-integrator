using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using NetTopologySuite.Geometries;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests
{
    public class RouteSegmentTest
    {
        [Fact]
        public void RouteSegment_ShouldSetInitialValues_OnConstruction()
        {
            var coord = Convert.FromBase64String("23022322/3232022");
            var mrid = Guid.Parse("053dc6c7-9210-4fbd-b564-f1357bcaf952");
            var username = "gdb-integrator";
            var workTaskMrid = Guid.Parse("8b97d7e6-7d45-4112-b3f3-2209fc3f27d5");
            var applicationName = "gdb-integrator";
            var applicationInfo = "Info text";
            var markAsDeleted = false;

            var routeSegment = new RouteSegment
            {
                Coord = coord,
                Mrid = mrid,
                Username = username,
                WorkTaskMrid = workTaskMrid,
                ApplicationName = applicationName,
                ApplicationInfo = applicationInfo,
                MarkAsDeleted = markAsDeleted,
            };

            using (new AssertionScope())
            {
                routeSegment.Coord.Should().BeEquivalentTo(coord);
                routeSegment.Mrid.Should().Be(mrid);
                routeSegment.Username.Should().Be(username);
                routeSegment.WorkTaskMrid.Should().Be(workTaskMrid);
                routeSegment.ApplicationName.Should().Be(applicationName);
                routeSegment.ApplicationInfo.Should().Be(applicationInfo);
                routeSegment.MarkAsDeleted.Should().Be(markAsDeleted);
            }
        }

        [Fact]
        public void FindStartPoint_ShouldReturnNewRouteNodeWithCalculatedCoord_OnBeingCalled()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var startPoint = routeSegment.FindStartPoint();

            var expected = new Point(565931.4446905176, 6197297.75114815);

            using (new AssertionScope())
            {
                startPoint.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void FindEndPoint_ShouldReturnNewRouteNodeWithCalculatedCoord_OnBeingCalled()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var endNode = routeSegment.FindEndPoint();

            var expected = new Point(565918.5429759022, 6197274.965055402);

            using (new AssertionScope())
            {
                endNode.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineData("AQIAACDoZAAAAgAAAEu6uBZURSFBSjSrSRikV0HVZEzDYkUhQZKMQUgZpFdB",
                    "[[565930.0443781106,6197345.151074478],[565937.381442214,6197349.1290008]]")]
        [InlineData("AQIAACDoZAAAAwAAAPWoEABERSFBRyt5XRqkV0Gans2pTEUhQcoJieAYpFdBS7vrn1dFIUFqP43oGaRXQQ==",
                    "[[565922.0001271056,6197353.460520572],[565926.3316468776,6197347.50836415],[565931.8123453645,6197351.633621076]]")]
        [InlineData("AQIAACDoZAAABAAAADW/1D1HRSFB6AnTgBWkV0FmW1lvUEUhQahJhp4UpFdBQeY1iklFIUGI6V8tFKRXQQ5MF2tHRSFBFZAntRSkV0E=",
                    "[[565923.620763755,6197334.01288078],[565928.217478615,6197330.476946272],[565924.76994247,6197328.708979018],[565923.7091621177,6197330.830539723]]")]
        public void GetGeoJsonCoordinate_ShouldReturnGeoJsonCoordinate_OnBeingCalled(string coord, string expected)
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String(coord),
            };

            var geoJson = routeSegment.GetGeoJsonCoordinate();

            geoJson.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLineString_ShouldReturnLineString_OnBeingPassedLineString()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAAAgAAAEu6uBZURSFBSjSrSRikV0HVZEzDYkUhQZKMQUgZpFdB")
            };

            var result = routeSegment.GetLineString();

            result.AsText().Should().BeEquivalentTo("LINESTRING (565930.04437811056 6197345.1510744784, 565937.381442214 6197349.1290008)");
        }
    }
}
