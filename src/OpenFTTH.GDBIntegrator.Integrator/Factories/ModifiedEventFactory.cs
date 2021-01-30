using System;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class ModifiedEventFactory : IModifiedEventFactory
    {
        const string ROUTE_NODE = "RouteNode";
        const string ROUTE_SEGMENT = "RouteSegment";

        public RouteSegmentInfoModified CreateRouteSegmentInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be passed in as null.");

            return new RouteSegmentInfoModified(
                 nameof(RouteSegmentInfoModified),
                 Guid.NewGuid(),
                 DateTime.UtcNow,
                 routeSegment.ApplicationName,
                 routeSegment.ApplicationInfo,
                 routeSegment.Mrid,
                 new RouteSegmentInfo
                 {
                     Width = String.IsNullOrEmpty(routeSegment.RouteSegmentInfo?.Width) ? null : routeSegment.RouteSegmentInfo.Width,
                     Height = String.IsNullOrEmpty(routeSegment.RouteSegmentInfo?.Height) ? null : routeSegment.RouteSegmentInfo.Height,
                     Kind = routeSegment.RouteSegmentInfo?.Kind,
                 });
        }

        public RouteNodeInfoModified CreateRouteNodeInfoModified(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new RouteNodeInfoModified(
                nameof(RouteNodeInfoModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode.ApplicationName,
                routeNode.ApplicationInfo,
                routeNode.Mrid,
                new RouteNodeInfo
                {
                    Function = routeNode.RouteNodeInfo?.Function,
                    Kind = routeNode.RouteNodeInfo?.Kind
                }
            );
        }

        public LifecycleInfoModified CreateLifeCycleInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be passsed in as null.");

            return new LifecycleInfoModified(
                nameof(LifecycleInfoModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment.ApplicationName,
                routeSegment.ApplicationInfo,
                routeSegment.Mrid,
                ROUTE_SEGMENT,
                new LifecycleInfo
                {
                    DeploymentState = routeSegment.LifeCycleInfo?.DeploymentState,
                    InstallationDate = routeSegment.LifeCycleInfo?.InstallationDate,
                    RemovalDate = routeSegment.LifeCycleInfo?.RemovalDate
                }
            );
        }

        public LifecycleInfoModified CreateLifeCycleInfoModified(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new LifecycleInfoModified(
                nameof(LifecycleInfoModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode.ApplicationName,
                routeNode.ApplicationInfo,
                routeNode.Mrid,
                ROUTE_NODE,
                new LifecycleInfo
                {
                    DeploymentState = routeNode.LifeCycleInfo?.DeploymentState,
                    InstallationDate = routeNode.LifeCycleInfo?.InstallationDate,
                    RemovalDate = routeNode.LifeCycleInfo?.RemovalDate
                }
            );
        }

        public MappingInfoModified CreateMappingInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new MappingInfoModified(
                nameof(MappingInfoModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment.ApplicationName,
                routeSegment.ApplicationInfo,
                routeSegment.Mrid,
                ROUTE_SEGMENT,
                new MappingInfo
                {
                    HorizontalAccuracy = string.IsNullOrEmpty(routeSegment.MappingInfo?.HorizontalAccuracy) ? null : routeSegment.MappingInfo.HorizontalAccuracy,
                    Method = routeSegment.MappingInfo?.Method,
                    SourceInfo = string.IsNullOrEmpty(routeSegment.MappingInfo?.SourceInfo) ? null : routeSegment.MappingInfo.SourceInfo,
                    SurveyDate = routeSegment.MappingInfo?.SurveyDate,
                    VerticalAccuracy = string.IsNullOrEmpty(routeSegment.MappingInfo?.VerticalAccuracy) ? null : routeSegment.MappingInfo.VerticalAccuracy
                }
            );
        }

        public MappingInfoModified CreateMappingInfoModified(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new MappingInfoModified(
                nameof(MappingInfoModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode.ApplicationName,
                routeNode.ApplicationInfo,
                routeNode.Mrid,
                ROUTE_NODE,
                new MappingInfo
                {
                    HorizontalAccuracy = routeNode.MappingInfo?.HorizontalAccuracy,
                    Method = routeNode.MappingInfo?.Method,
                    SourceInfo = string.IsNullOrEmpty(routeNode.MappingInfo?.SourceInfo) ? null : routeNode.MappingInfo.SourceInfo,
                    SurveyDate = routeNode.MappingInfo?.SurveyDate,
                    VerticalAccuracy = String.IsNullOrEmpty(routeNode.MappingInfo?.VerticalAccuracy) ? null : routeNode.MappingInfo.VerticalAccuracy
                }
            );
        }

        public SafetyInfoModified CreateSafetyInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be passsed in as null.");

            return new SafetyInfoModified(
                    nameof(SafetyInfoModified),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    routeSegment.ApplicationName,
                    routeSegment.ApplicationInfo,
                    routeSegment.Mrid,
                    ROUTE_SEGMENT,
                    new SafetyInfo
                    {
                        Classification = String.IsNullOrEmpty(routeSegment.SafetyInfo?.Classification) ? null : routeSegment.SafetyInfo.Classification,
                        Remark = String.IsNullOrEmpty(routeSegment.SafetyInfo?.Remark) ? null : routeSegment.SafetyInfo.Remark
                    }
                );
        }

        public SafetyInfoModified CreateSafetyInfoModified(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new SafetyInfoModified(
                    nameof(SafetyInfoModified),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    routeNode.ApplicationName,
                    routeNode.ApplicationInfo,
                    routeNode.Mrid,
                    ROUTE_NODE,
                    new SafetyInfo
                    {
                        Classification = String.IsNullOrEmpty(routeNode.SafetyInfo?.Classification) ? null : routeNode.SafetyInfo.Classification,
                        Remark = String.IsNullOrEmpty(routeNode.SafetyInfo?.Remark) ? null : routeNode.SafetyInfo.Remark
                    }
                );
        }

        public NamingInfoModified CreateNamingInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be passsed in as null.");

            return new NamingInfoModified(
                    nameof(NamingInfoModified),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    routeSegment.ApplicationName,
                    routeSegment.ApplicationInfo,
                    routeSegment.Mrid,
                    ROUTE_SEGMENT,
                    new NamingInfo
                    {
                        Description = String.IsNullOrEmpty(routeSegment.NamingInfo?.Description) ? null : routeSegment.NamingInfo.Description,
                        Name = string.IsNullOrEmpty(routeSegment.NamingInfo?.Name) ? null : routeSegment.NamingInfo.Name
                    }
                );
        }

        public NamingInfoModified CreateNamingInfoModified(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"{nameof(RouteNode)} cannot be passsed in as null.");

            return new NamingInfoModified(
                    nameof(NamingInfoModified),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    routeNode.ApplicationName,
                    routeNode.ApplicationInfo,
                    routeNode.Mrid,
                    ROUTE_NODE,
                    new NamingInfo
                    {
                        Description = String.IsNullOrEmpty(routeNode.NamingInfo?.Description) ? null : routeNode.NamingInfo.Description,
                        Name = String.IsNullOrEmpty(routeNode.NamingInfo?.Name) ? null : routeNode.NamingInfo.Name
                    }
                );
        }
    }
}
