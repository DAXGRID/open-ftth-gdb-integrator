using System;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeCommandFactory : IRouteNodeCommandFactory
    {
        private readonly IMediator _mediator;
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeCommandFactory(IMediator mediator, IGeoDatabase geoDatabase)
        {
            _mediator = mediator;
            _geoDatabase = geoDatabase;
        }

        public async Task<IRequest> Create(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);


            if (intersectingRouteSegments.Count == 0)
            {
                return new NewLonelyRouteNodeCommand { RouteNode = routeNode };
            }

            throw new Exception("No valid event for current state");
        }
    }
}
