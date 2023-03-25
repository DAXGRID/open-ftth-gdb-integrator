using NetTopologySuite.IO;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using System;
using System.Linq;
using System.Text.Json;

namespace OpenFTTH.GDBIntegrator.Subscriber.Postgres;

public sealed class RouteSegmentMessageFactory
{
    private readonly IInfoMapper _infoMapper;

    public RouteSegmentMessageFactory(IInfoMapper infoMapper)
    {
        _infoMapper = infoMapper;
    }

    public RouteSegmentMessage Create(RouteNetworkEditOperation editOperation)
    {
        if (editOperation is null)
        {
            throw new ArgumentNullException(nameof(editOperation));
        }

        if (editOperation.Type != RouteNetworkEditTypeName.RouteSegment)
        {
            throw new ArgumentException(
                $"Cannot create {nameof(RouteSegmentMessage)} from {editOperation.Type}.");
        }

        return new RouteSegmentMessage(
            eventId: editOperation.EventId,
            before: ParseRouteSegment(editOperation.Before),
            after: ParseRouteSegment(editOperation.After)
        );
    }

    private RouteSegment ParseRouteSegment(string json)
    {
        if (String.IsNullOrEmpty(json))
        {
            return null;
        }

        var pgRouteSegment = JsonSerializer.Deserialize<PostgresRouteSegment>(json);
        if (pgRouteSegment is null)
        {
            return null;
        }

        var wkbWriter = new WKBWriter();

        var mappedRouteSegment = new RouteSegment
        {
            ApplicationInfo = pgRouteSegment.ApplicationInfo,
            ApplicationName = pgRouteSegment.ApplicationName,
            Coord = pgRouteSegment.Coord is null ? null : wkbWriter.Write(pgRouteSegment.Coord),
            MarkAsDeleted = pgRouteSegment.MarkedToBeDeleted,
            DeleteMe = pgRouteSegment.DeleteMe,
            Mrid = pgRouteSegment.Mrid,
            Username = pgRouteSegment.UserName,
            WorkTaskMrid = pgRouteSegment.WorkTaskMrid ?? Guid.Empty,
            LifeCycleInfo = new LifecycleInfo(
                _infoMapper.MapDeploymentState(pgRouteSegment.LifecycleDeploymentState),
                 pgRouteSegment.LifecycleInstallationDate,
                 pgRouteSegment.LifecycleRemovalDate
            ),
            MappingInfo = new MappingInfo(
                _infoMapper.MapMappingMethod(pgRouteSegment.MappingMethod),
                pgRouteSegment.MappingVerticalAccuracy,
                pgRouteSegment.MappingHorizontalAccuracy,
                pgRouteSegment.MappingSurveyDate,
                pgRouteSegment.MappingSourceInfo
            ),
            NamingInfo = new NamingInfo(
                pgRouteSegment.NamingName,
                pgRouteSegment.NamingDescription
            ),
            RouteSegmentInfo = new RouteSegmentInfo(
                _infoMapper.MapRouteSegmentKind(pgRouteSegment.RoutesegmentKind),
                pgRouteSegment.RouteSegmentWidth,
                pgRouteSegment.RouteSegmentHeight
            ),
            SafetyInfo = new SafetyInfo(
                pgRouteSegment.SafetyClassification,
                pgRouteSegment.SafetyRemark
            )
        };

        // Make fully empty objects into nulls.
        mappedRouteSegment.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(mappedRouteSegment.LifeCycleInfo) ? mappedRouteSegment.LifeCycleInfo : null;
        mappedRouteSegment.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(mappedRouteSegment.MappingInfo) ? mappedRouteSegment.MappingInfo : null;
        mappedRouteSegment.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(mappedRouteSegment.NamingInfo) ? mappedRouteSegment.NamingInfo : null;
        mappedRouteSegment.RouteSegmentInfo = AreAnyPropertiesNotNull<RouteSegmentInfo>(mappedRouteSegment.RouteSegmentInfo) ? mappedRouteSegment.RouteSegmentInfo : null;
        mappedRouteSegment.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(mappedRouteSegment.SafetyInfo) ? mappedRouteSegment.SafetyInfo : null;


        return mappedRouteSegment;
    }

    private bool AreAnyPropertiesNotNull<T>(object obj)
    {
        return typeof(T)
            .GetProperties()
            .Any(propertyInfo => propertyInfo.GetValue(obj) != null);
    }
}
