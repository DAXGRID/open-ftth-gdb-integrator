using System;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class ModifiedEventFactory : IModifiedEventFactory
    {
        public RouteSegmentInfoModified CreateRouteSegmentInfoModified(RouteSegment routeSegment)
        {
            if (routeSegment is null)
                throw new ArgumentNullException($"{nameof(RouteSegment)} cannot be passed in as null.");

            return new RouteSegmentInfoModified(
                 nameof(RouteSegmentInfoModified),
                 Guid.NewGuid(),
                 DateTime.UtcNow,
                 routeSegment?.ApplicationName,
                 routeSegment?.ApplicationInfo,
                 routeSegment.Mrid,
                 new RouteSegmentInfo
                 {
                     Width = routeSegment.RouteSegmentInfo?.Width,
                     Height = routeSegment.RouteSegmentInfo?.Height,
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
                routeNode?.ApplicationName,
                routeNode?.ApplicationInfo,
                routeNode.Mrid,
                new RouteNodeInfo
                {
                    Function = routeNode.RouteNodeInfo?.Function,
                    Kind = routeNode.RouteNodeInfo?.Kind
                }
            );
        }
    }
}
