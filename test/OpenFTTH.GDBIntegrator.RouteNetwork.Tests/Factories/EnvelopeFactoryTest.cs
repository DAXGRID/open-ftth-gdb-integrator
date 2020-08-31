using System;
using System.Collections.Generic;
using Xunit;
using OpenFTTH.GDBIntegrator.RouteNetwork.Factories;
using OpenFTTH.GDBIntegrator.Config;
using FluentAssertions;
using FluentAssertions.Execution;
using FakeItEasy;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Test.Factories
{
    public class EnvelopeFactoryTest
    {
        [Fact]
        public void Create_ShouldReturnEnvelope_OnBeingPassedSingleRouteNodesAndSingleRouteSegments()
        {
            var routeSegment = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var routeNode = new RouteNode
            {
                Coord = Convert.FromBase64String("AQEAAAC8ea7jVkUhQbHPEnAMpFdB")
            };

            var routeNodes = new List<RouteNode> { routeNode };
            var routeSegments = new List<RouteSegment> { routeSegment };


            var envelopeFactory = new EnvelopeFactory();
            var result = envelopeFactory.Create(routeNodes, routeSegments);


            using (var scope = new AssertionScope())
            {
                result.MinX.Should().Be(565918.5429759022);
                result.MaxX.Should().Be(565985.2365167774);
                result.MinY.Should().Be(6197265.913045954);
                result.MaxY.Should().Be(6197319.496780043);
            }
        }

        [Fact]
        public void Create_ShouldReturnEnvelope_OnBeingPassedMultipleRouteNodesAndMultipleRouteSegments()
        {
            var routeSegmentOne = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABgAAALx5ruNWRSFBsc8ScAykV0HZ6xJ8lEUhQYU+y98RpFdBILoYecJFIUEVfnDVB6RXQZH1zbVhRSFBTFhvegSkV0G/QerRbkUhQYWC7LEKpFdB/e8AFj1FIUG8d8O9BqRXQQ=="),
            };

            var routeSegmentTwo = new RouteSegment
            {
                Coord = Convert.FromBase64String("AQIAACDoZAAABAAAADW/1D1HRSFB6AnTgBWkV0FmW1lvUEUhQahJhp4UpFdBQeY1iklFIUGI6V8tFKRXQQ5MF2tHRSFBFZAntRSkV0E=")
            };

            var routeNodeOne = new RouteNode
            {
                Coord = Convert.FromBase64String("AQEAAAC8ea7jVkUhQbHPEnAMpFdB")
            };

            var routeNodeTwo = new RouteNode
            {
                Coord = Convert.FromBase64String("AQEAAAC8ea7jVkUhQbHPEnAMpFdB")
            };

            var routeNodes = new List<RouteNode> { routeNodeOne };
            var routeSegments = new List<RouteSegment> { routeSegmentOne, routeSegmentTwo};


            var envelopeFactory = new EnvelopeFactory();
            var result = envelopeFactory.Create(routeNodes, routeSegments);


            using (var scope = new AssertionScope())
            {
                result.MinX.Should().Be(565918.5429759022);
                result.MaxX.Should().Be(565985.2365167774);
                result.MinY.Should().Be(6197265.913045954);
                result.MaxY.Should().Be(6197334.01288078);
            }
        }
    }
}
