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
                notifications.Add(new RollbackInvalidRouteNode(before));
                return notifications;
            }

            var routeNodeShadowTable = await _geoDatabase.GetRouteNodeShadowTable(after.Mrid, true);

            if (AlreadyUpdated(after, routeNodeShadowTable))
            {
                notifications.Add(new DoNothing($"{nameof(RouteNode)} is already updated, therefore do nothing."));
                return notifications;
            }

            if (before.MarkAsDeleted)
            {
                notifications.Add(new RollbackInvalidRouteNode(before));
                return notifications;
            }

            if (IsRouteNodeInfoUpdated(before, after))
            {
                notifications.Add(new RouteNodeInfoUpdated(after));
            }

            if (IsLifecycleInfoModified(before, after))
            {
                notifications.Add(new RouteNodeLifecycleInfoUpdated(after));
            }

            if (IsMappingInfoModified(before, after))
            {
                notifications.Add(new RouteNodeMappingInfoUpdated(after));
            }

            if (IsNamingInfoModified(before, after))
            {
                notifications.Add(new RouteNodeNamingInfoUpdated(after));
            }

            if (IsSafetyInfoModified(before, after))
            {
                notifications.Add(new RouteNodeSafetyInfoUpdated(after));
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

            return false;
        }

        private bool IsLifecycleInfoModified(RouteNode before, RouteNode after)
        {
            if (before.LifeCycleInfo?.DeploymentState != after.LifeCycleInfo?.DeploymentState ||
                before.LifeCycleInfo?.InstallationDate != after.LifeCycleInfo?.InstallationDate ||
                before.LifeCycleInfo?.RemovalDate != after.LifeCycleInfo?.RemovalDate)
            {
                return true;
            }

            return false;
        }

        private bool IsMappingInfoModified(RouteNode before, RouteNode after)
        {
            if (before.MappingInfo?.HorizontalAccuracy != after.MappingInfo?.HorizontalAccuracy ||
                before.MappingInfo?.Method != after.MappingInfo?.Method ||
                before.MappingInfo?.SourceInfo != after.MappingInfo?.SourceInfo ||
                before.MappingInfo?.SurveyDate != after.MappingInfo?.SurveyDate ||
                before.MappingInfo?.VerticalAccuracy != after.MappingInfo?.VerticalAccuracy)
            {
                return true;
            }

            return false;
        }

        private bool IsNamingInfoModified(RouteNode before, RouteNode after)
        {
            if (before.NamingInfo?.Description != after.NamingInfo?.Description ||
                before.NamingInfo?.Name != after.NamingInfo?.Name)
            {
                return true;
            }

            return false;
        }

        private bool IsSafetyInfoModified(RouteNode before, RouteNode after)
        {
            if (before.SafetyInfo?.Classification != after.SafetyInfo?.Classification ||
            before.SafetyInfo?.Remark != after.SafetyInfo?.Remark)
            {
                return true;
            }

            return false;
        }

        private bool AlreadyUpdated(RouteNode after, RouteNode shadowTableRouteNode)
        {
            return !IsNamingInfoModified(after, shadowTableRouteNode)
                && !IsNamingInfoModified(after, shadowTableRouteNode)
                && !IsMappingInfoModified(after, shadowTableRouteNode)
                && !IsLifecycleInfoModified(after, shadowTableRouteNode)
                && !IsRouteNodeInfoUpdated(after, shadowTableRouteNode)
                && !IsSafetyInfoModified(after, shadowTableRouteNode);
        }
    }
}
