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

public sealed class RouteNodeMessageFactory
{
    private readonly IInfoMapper _infoMapper;

    public RouteNodeMessageFactory(IInfoMapper infoMapper)
    {
        _infoMapper = infoMapper;
    }

    public RouteNodeMessage Create(RouteNetworkEditOperation editOperation)
    {
        if (editOperation is null)
        {
            throw new ArgumentNullException(nameof(editOperation));
        }

        if (editOperation.Type != RouteNetworkEditTypeName.RouteNode)
        {
            throw new ArgumentException(
                $"Cannot create {nameof(RouteNodeMessage)} from {editOperation.Type}.");
        }

        return new RouteNodeMessage(
            eventId: editOperation.EventId,
            before: ParseRouteNode(editOperation.Before),
            after: ParseRouteNode(editOperation.After)
        );
    }

    private RouteNode ParseRouteNode(string json)
    {
        if (String.IsNullOrEmpty(json))
        {
            return null;
        }

        var pgRouteNode = JsonSerializer.Deserialize<PostgresRouteNode>(json);
        if (pgRouteNode is null)
        {
            return null;
        }

        var wkbWriter = new WKBWriter();

        var mappedRouteNode = new RouteNode
        {
            ApplicationInfo = pgRouteNode.ApplicationInfo,
            ApplicationName = pgRouteNode.ApplicationName,
            Coord = pgRouteNode.Coord is null ? null : wkbWriter.Write(pgRouteNode.Coord),
            MarkAsDeleted = pgRouteNode.MarkedToBeDeleted,
            DeleteMe = pgRouteNode.DeleteMe,
            Mrid = pgRouteNode.Mrid,
            Username = pgRouteNode.UserName,
            WorkTaskMrid = pgRouteNode.WorkTaskMrid ?? Guid.Empty,
            LifeCycleInfo = new LifecycleInfo(
                _infoMapper.MapDeploymentState(pgRouteNode.LifecycleDeploymentState),
                 pgRouteNode.LifecycleInstallationDate,
                 pgRouteNode.LifecycleRemovalDate
            ),
            MappingInfo = new MappingInfo(
                _infoMapper.MapMappingMethod(pgRouteNode.MappingMethod),
                pgRouteNode.MappingVerticalAccuracy,
                pgRouteNode.MappingHorizontalAccuracy,
                pgRouteNode.MappingSurveyDate,
                pgRouteNode.MappingSourceInfo
            ),
            NamingInfo = new NamingInfo(
                pgRouteNode.NamingName,
                pgRouteNode.NamingDescription
            ),
            RouteNodeInfo = new RouteNodeInfo(
                _infoMapper.MapRouteNodeKind(pgRouteNode.RouteNodeKind),
                _infoMapper.MapRouteNodeFunction(pgRouteNode.RoutenodeFunction)
            ),
            SafetyInfo = new SafetyInfo(
                pgRouteNode.SafetyClassification,
                pgRouteNode.SafetyRemark
            )
        };

        // Make fully empty objects into nulls.
        mappedRouteNode.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(mappedRouteNode.LifeCycleInfo) ? mappedRouteNode.LifeCycleInfo : null;
        mappedRouteNode.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(mappedRouteNode.MappingInfo) ? mappedRouteNode.MappingInfo : null;
        mappedRouteNode.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(mappedRouteNode.NamingInfo) ? mappedRouteNode.NamingInfo : null;
        mappedRouteNode.RouteNodeInfo = AreAnyPropertiesNotNull<RouteNodeInfo>(mappedRouteNode.RouteNodeInfo) ? mappedRouteNode.RouteNodeInfo : null;
        mappedRouteNode.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(mappedRouteNode.SafetyInfo) ? mappedRouteNode.SafetyInfo : null;

        return mappedRouteNode;
    }

    private bool AreAnyPropertiesNotNull<T>(object obj)
    {
        return typeof(T)
            .GetProperties()
            .Any(propertyInfo => propertyInfo.GetValue(obj) != null);
    }
}
