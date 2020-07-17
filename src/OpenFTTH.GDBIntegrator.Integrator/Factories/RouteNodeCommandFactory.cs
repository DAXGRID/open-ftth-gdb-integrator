using System;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeCommandFactory : IRouteNodeCommandFactory
    {
        private readonly IMediator _mediator;
        private readonly IGeoDatabase _geoDatabase;
        private readonly ApplicationSetting _applicationSetting;

        public RouteNodeCommandFactory(
            IMediator mediator,
            IGeoDatabase geoDatabase,
            IOptions<ApplicationSetting> applicationSetting)
        {
            _mediator = mediator;
            _geoDatabase = geoDatabase;
            _applicationSetting = applicationSetting.Value;
        }

        public async Task<IRequest> Create(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            // If the GDB integrator produced the message do nothing
            if (routeNode.ApplicationName == _applicationSetting.ApplicationName)
                return new GdbCreatedEntity();

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);

            if (intersectingRouteSegments.Count == 0)
                return new NewLonelyRouteNode { RouteNode = routeNode };

            if (intersectingRouteSegments.Count == 1)
                return new ExistingRouteSegmentSplittedByRouteNode { RouteNode = routeNode, IntersectingRouteSegment = intersectingRouteSegments.First() };

            return new InvalidRouteNodeOperation { RouteNode = routeNode };
        }
    }
}
