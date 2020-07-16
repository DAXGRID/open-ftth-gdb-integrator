using System;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using System.Threading.Tasks;
using System.Threading;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeCommandFactory : IRouteNodeCommandFactory
    {
        private readonly IMediator _mediator;

        public RouteNodeCommandFactory(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IRequest> Create(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            return new NewLonelyRouteNodeCommand { RouteNode = routeNode };
        }
    }
}
