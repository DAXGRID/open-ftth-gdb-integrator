using Xunit;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using FakeItEasy;
using System;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests.Validators
{
    public class RouteSegmentValidatorTest
    {
        [Fact]
        public void LineIsValid_ShouldReturnTrue_OnSimpleLine()
        {
            //  O------O
           
            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((true, (string)null));
        }

        [Fact]
        public void LineIsValid_ShouldReturnFalse_OnEndsSnappedLine()
        {
            //  Line with ends snapped together, which is not okay
            //
            //  -------O
            //  |      |
            //  |      |
            //  |------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578241.656539916 6179263.6946997,578230.221332537 6179263.2899136,578229.715349909 6179272.70119047,578241.352950339 6179273.40956615,578241.656539916 6179263.6946997)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((false, "LINE_STRING_IS_CLOSED"));
        }

        [Fact]
        public void LineIsValid_ShouldReturnFalse_OnSelfIntersectingLine()
        {
            //  Line self interects, which is not okay
            //
            //         O
            //         |
            //  -----------O
            //  |      |
            //  |      |
            //  |------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578246.766964452 6179292.47246163,578228.753982917 6179292.77605121,578229.867144697 6179305.830403,578241.909531229 6179304.81843774,578239.076028516 6179286.70425968)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((false, "LINE_STRING_IS_NOT_SIMPLE"));
        }

        [Fact]
        public void LineIsValid_ShouldReturnFalse_OnEndSnappedToEdge()
        {
            //  Line end snapped to line edge, which is not ok
            //
            //         O
            //         |
            //  -------O
            //  |      |
            //  |      |
            //  |------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578268.32182438 6179249.86872441,578252.029183777 6179249.76752788,578252.332773354 6179266.56615111,578263.261998106 6179266.56615111,578263.667592312 6179249.83981613)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((false, "LINE_STRING_IS_NOT_SIMPLE"));
        }

        [Fact]
        public void LineIsValid_ShouldReturnTrue_OnEndsDistanceAtTolerance()
        {
            //  Line ends 0.01 (tolerance) meters from each other
            //
            //  |-O < 0.01 > O-|
            //  |              |
            //  |              |
            //  |--------------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578257.898582255 6179230.85514246)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((true, (string)null));
        }

        [Fact]
        public void LineIsValid_ShouldReturnFalse_OnEndsDistanceLessThanTolerance()
        {
            //  Line ends are 0.005 (tolerance) meters from each other
            //
            //  |-O < 0.005 > O-|
            //  |               |
            //  |               |
            //  |---------------|
            
            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578257.898186956 6179230.84901533)") as LineString;

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);
            result.Should().BeEquivalentTo((false, "LINE_STRING_ENDS_CLOSER_TO_EACH_OTHER_THAN_TOLERANCE"));
        }

        [Fact]
        public void LineIsValid_ShouldReturnTrue_OnEdgeDistanceMoreThanTolerance()
        {
            //  Line end 0.023 from edge is ok
            //
            //              S
            //              |
            //|--E < 0.023 >|
            //|             |
            //|             |
            //|-------------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578256.8130914 6179230.85534011)") as LineString;

            Coordinate[] linePointsMinusOne = new Coordinate[line.NumPoints - 1];

            for (int i = 0; i < line.NumPoints - 1; i++)
                linePointsMinusOne[i] = line.GetPointN(i).Coordinate;

            LineString newLine = new LineString(linePointsMinusOne);

            var endPointToEdgeDistance = Math.Round(line.EndPoint.Distance(newLine), 3);

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);

            // Assert that distance from end point to edge is 0.023
            endPointToEdgeDistance.Should().Be(0.023);
            result.Should().BeEquivalentTo((true, (string)null));
        }

        [Fact]
        public void LineIsValid_ShouldReturnFalse_OnEdgeDistanceMoreLessTolerance()
        {
            //  Line end 0.023 from edge is not ok
            //
            //              S
            //              |
            //|--E < 0.007 >|
            //|             |
            //|             |
            //|-------------|

            var logger = A.Fake<ILogger<RouteSegmentValidator>>();
            var applicationSettings = A.Fake<IOptions<ApplicationSetting>>();
            A.CallTo(() => applicationSettings.Value).Returns(new ApplicationSetting { Tolerance = 0.01 });

            var line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578256.811707854 6179230.83918227)") as LineString;

            // Assert that distance from end point to edge is 0.007
            Coordinate[] linePointsMinusOne = new Coordinate[line.NumPoints - 1];

            for (int i = 0; i < line.NumPoints - 1; i++)
                linePointsMinusOne[i] = line.GetPointN(i).Coordinate;

            var newLine = new LineString(linePointsMinusOne);

            var endPointToEdgeDistance = Math.Round(line.EndPoint.Distance(newLine), 3);

            var routeSegmentValidator = new RouteSegmentValidator(logger, applicationSettings);

            var result = routeSegmentValidator.LineIsValid(line);

            // Assert that distance from end point to edge is 0.007
            endPointToEdgeDistance.Should().Be(0.007);
            result.Should().BeEquivalentTo((false, "LINE_STRING_ENDS_CLOSER_TO_THE_EDGE_THAN_TOLERANCE"));;
        }
    }
}
