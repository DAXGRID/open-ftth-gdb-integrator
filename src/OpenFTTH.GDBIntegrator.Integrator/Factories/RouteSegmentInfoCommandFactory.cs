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
    public class RouteSegmentInfoCommandFactory : IRouteSegmentInfoCommandFactory
    {
        private readonly IGeoDatabase _geoDatabase;

        public RouteSegmentInfoCommandFactory(IGeoDatabase geoDatabase)
        {
            _geoDatabase = geoDatabase;
        }

        public async Task<IEnumerable<INotification>> Create(RouteSegment before, RouteSegment after)
        {
            var notifications = new List<INotification>();

            if (before is null || after is null)
            {
                throw new ArgumentNullException(
                    $"Parameter {nameof(before)} or {nameof(after)} cannot be null");
            }

            if (IsRouteSegmentInfoUpdated(before, after))
            {
                notifications.Add(new RouteSegmentInfoUpdated(after));
            }

            if (IsLifecycleInfoModified(before, after))
            {
                notifications.Add(new RouteSegmentLifecycleInfoUpdated(after));
            }

            if (IsMappingInfoModified(before, after))
            {
                notifications.Add(new RouteSegmentMappingInfoUpdated(after));
            }

            // if (IsNamingInfoModified(before, after))
            // {
            //     notifications.Add(new RouteNodeNamingInfoUpdated(after));
            // }

            // if (IsSafetyInfoModified(before, after))
            // {
            //     notifications.Add(new RouteNodeSafetyInfoUpdated(after));
            // }

            if (notifications.Any())
            {
                await _geoDatabase.UpdateRouteSegmentInfosShadowTable(after);
            }

            return notifications;
        }

        private bool IsRouteSegmentInfoUpdated(RouteSegment before, RouteSegment after)
        {
            if (before.RouteSegmentInfo?.Height != after.RouteSegmentInfo?.Height ||
                before.RouteSegmentInfo?.Kind != after.RouteSegmentInfo?.Kind ||
                before.RouteSegmentInfo?.Width != after.RouteSegmentInfo?.Width)
            {
                return true;
            }

            return false;
        }

        private bool IsLifecycleInfoModified(RouteSegment before, RouteSegment after)
        {
            if (before.LifeCycleInfo?.DeploymentState != after.LifeCycleInfo?.DeploymentState ||
                before.LifeCycleInfo?.InstallationDate != after.LifeCycleInfo?.InstallationDate ||
                before.LifeCycleInfo?.RemovalDate != after.LifeCycleInfo?.RemovalDate)
            {
                return true;
            }

            return false;
        }

        private bool IsMappingInfoModified(RouteSegment before, RouteSegment after)
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

        private bool IsNamingInfoModified(RouteSegment before, RouteSegment after)
        {
            if (before.NamingInfo?.Description != after.NamingInfo?.Description ||
                before.NamingInfo?.Name != after.NamingInfo?.Name)
            {
                return true;
            }

            return false;
        }

        private bool IsSafetyInfoModified(RouteSegment before, RouteSegment after)
        {
            if (before.SafetyInfo?.Classification != after.SafetyInfo?.Classification ||
            before.SafetyInfo?.Remark != after.SafetyInfo?.Remark)
            {
                return true;
            }

            return false;
        }
    }
}
