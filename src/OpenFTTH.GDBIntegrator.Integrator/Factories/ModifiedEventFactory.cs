using System;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class ModifiedEventFactory : IModifiedEventFactory
    {
        public RouteSegmentInfoModified CreateRouteSegmentInfoModified(RouteSegment routeSegment) => new RouteSegmentInfoModified(
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
}
