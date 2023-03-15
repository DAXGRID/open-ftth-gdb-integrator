using Xunit;
using NetTopologySuite.Geometries;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using FakeItEasy;
using System;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests.Validators
{
    public class RouteNodeValidatorTest
    {
        [Fact]
        public void PointIsValid_ShouldReturnTrue_OnPointHavingValidCoordinate()
        {
            var logger = A.Fake<ILogger<RouteNodeValidator>>();

            var point = new Point(555921, 6342366);

            var routeNodeValidator = new RouteNodeValidator(logger);

            var result = routeNodeValidator.PointIsValid(point);
            result.Should().BeTrue();
        }

        [Fact]
        public void PointIsValid_ShouldReturnFalse_OnPointBeingNaN()
        {
            var logger = A.Fake<ILogger<RouteNodeValidator>>();

            var point = new Point(Double.NaN, Double.NaN);

            var routeNodeValidator = new RouteNodeValidator(logger);

            var result = routeNodeValidator.PointIsValid(point);
            result.Should().BeFalse();
        }
    }
}
