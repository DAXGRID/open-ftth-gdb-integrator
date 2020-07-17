using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Validators;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentCommandFactory : IRouteSegmentCommandFactory
    {
        private readonly IMediator _mediator;
        private readonly IRouteSegmentValidator _routeSegmentValidator;

        public RouteSegmentCommandFactory(IMediator mediator, IRouteSegmentValidator routeSegmentValidator)
        {
            _mediator = mediator;
            _routeSegmentValidator = routeSegmentValidator;
        }

        public async Task<IRequest> Create(RouteSegment routeSegment)
        {
            if (!_routeSegmentValidator.LineIsValid(routeSegment.GetLineString()))
                return new InvalidRouteSegmentOperation { RouteSegment = routeSegment };

            var intersectingStartNodes = await _mediator.Send(
                new GetIntersectingStartRouteNodes { RouteSegment = routeSegment });
            var intersectingEndNodes = await _mediator.Send(
                new GetIntersectingEndRouteNodes { RouteSegment = routeSegment });

            var totalIntersectingNodes = intersectingStartNodes.Count + intersectingEndNodes.Count;

            if (totalIntersectingNodes == 0)
            {
                return new NewLonelyRouteSegment { RouteSegment = routeSegment };
            }
            else if (intersectingStartNodes.Count == 1 && intersectingEndNodes.Count == 1)
            {
                return new NewRouteSegmentBetweenTwoExistingNodes
                {
                    RouteSegment = routeSegment,
                    StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                    EndRouteNode = intersectingEndNodes.FirstOrDefault()
                };
            }
            else if (totalIntersectingNodes == 1)
            {
                return new NewRouteSegmentToExistingNode
                {
                    RouteSegment = routeSegment,
                    StartRouteNode = intersectingStartNodes.FirstOrDefault(),
                    EndRouteNode = intersectingEndNodes.FirstOrDefault()
                };
            }

            return new InvalidRouteSegmentOperation { RouteSegment = routeSegment };
        }
    }
}
