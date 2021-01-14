using System;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Store;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeEventFactory : IRouteNodeEventFactory
    {
        private readonly IModifiedGeometriesStore _modifiedGeomitries;

        public RouteNodeEventFactory(IModifiedGeometriesStore modifiedGeomitriesStore)
        {
            _modifiedGeomitries = modifiedGeomitriesStore;
        }

        public RouteNodeAdded CreateAdded(RouteNode routeNode)
        {
            _modifiedGeomitries.InsertRouteNode(routeNode);

            return new Events.RouteNetwork.RouteNodeAdded(
                nameof(Events.RouteNetwork.RouteNodeAdded),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode?.ApplicationName,
                routeNode?.ApplicationInfo,
                routeNode?.NamingInfo,
                routeNode?.LifeCycleInfo,
                routeNode?.MappingInfo,
                routeNode?.SafetyInfo,
                routeNode.Mrid,
                routeNode.GetGeoJsonCoordinate(),
                routeNode.RouteNodeInfo);
        }

        public RouteNodeMarkedForDeletion CreateMarkedForDeletion(RouteNode routeNode)
        {
            _modifiedGeomitries.InsertRouteNode(routeNode);

            return new RouteNodeMarkedForDeletion(
                nameof(RouteNodeMarkedForDeletion),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode?.ApplicationName,
                routeNode?.ApplicationInfo,
                routeNode.Mrid);
        }

        public RouteNodeGeometryModified CreateGeometryModified(RouteNode routeNode)
        {
            _modifiedGeomitries.InsertRouteNode(routeNode);

            return new RouteNodeGeometryModified(
                nameof(RouteNodeGeometryModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeNode?.ApplicationName,
                routeNode?.ApplicationInfo,
                routeNode.Mrid,
                routeNode.GetGeoJsonCoordinate());
        }
    }
}
