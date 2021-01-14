using System;
using System.Threading.Tasks;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeInfoCommandFactory
    {
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeInfoCommandFactory(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public Task<INotification> CreateInfoUpdatedEvent(RouteNode before, RouteNode after)
        {
            if (before is null || after is null)
                throw new ArgumentNullException($"Parameter {nameof(before)} or {nameof(after)} cannot be null");

            return null;
        }
    }
}
