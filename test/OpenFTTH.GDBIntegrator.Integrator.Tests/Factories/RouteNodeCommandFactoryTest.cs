using Xunit;
using FluentAssertions;
using FakeItEasy;
using OpenFTTH.GDBIntegrator.Integrator.Factories;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Tests.Factories
{
    public class RouteNodeCommandFactoryTest
    {
        [Fact]
        public async Task Create_ShouldThrowArgumentNullException_OnBeingPassedRouteNodeThatIsNull()
        {
            var mediator = A.Fake<IMediator>();
            RouteNode routeNode = null;
            var routeNodeCommandFactory = new RouteNodeCommandFactory(mediator);

            Func<Task> act = async () => { await routeNodeCommandFactory.Create(routeNode); };
            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
