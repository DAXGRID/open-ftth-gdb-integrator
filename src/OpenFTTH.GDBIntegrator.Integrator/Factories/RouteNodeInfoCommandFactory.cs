using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{

    public class RouteNodeInfoCommandFactory : IRouteNodeInfoCommandFactory
    {
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeInfoCommandFactory(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<IEnumerable<INotification>> Create(RouteNode before, RouteNode after)
        {
            var notifications = new List<INotification>();

            if (before is null || after is null)
            {
                throw new ArgumentNullException(
                    $"Parameter {nameof(before)} or {nameof(after)} cannot be null");
            }

            if (IsRouteNodeInfoUpdated(before, after))
            {
                notifications.Add(new RouteNodeInfoUpdated(after));
            }

            if (IsLifecycleInfoModified(before, after))
            {
                notifications.Add(new LifecycleInfoUpdated(after));
            }

            if (notifications.Any())
            {
                await _geoDatabase.UpdateRouteNodeInfosShadowTable(after);
            }

            return notifications;
        }

        private bool IsRouteNodeInfoUpdated(RouteNode before, RouteNode after)
        {
            if (before.RouteNodeInfo?.Function != after.RouteNodeInfo?.Function ||
                before.RouteNodeInfo?.Kind != after.RouteNodeInfo?.Kind)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsLifecycleInfoModified(RouteNode before, RouteNode after)
        {
            if (before.LifeCycleInfo?.DeploymentState != after.LifeCycleInfo?.DeploymentState ||
                before.LifeCycleInfo?.InstallationDate != after.LifeCycleInfo?.InstallationDate ||
                before.LifeCycleInfo?.RemovalDate != after.LifeCycleInfo?.RemovalDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
