using System;
using MediatR;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

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
                return new GdbCreatedEntityCommand();

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(routeNode);

            if (intersectingRouteSegments.Count == 0)
                return new NewLonelyRouteNodeCommand { RouteNode = routeNode };

            throw new Exception("No valid event for current state");
        }
    }
}
