using System;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeCommandFactory : IRouteNodeCommandFactory
    {
        private readonly IMediator _mediator;

        public RouteNodeCommandFactory(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<IRequest> Create(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            return null;
        }
    }
}
