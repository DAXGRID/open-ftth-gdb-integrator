using System;
using System.Collections.Generic;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using OpenFTTH.GDBIntegrator.Config;
using Microsoft.Extensions.Options;


namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public class RouteSegmentFactory : IRouteSegmentFactory
    {
        private ApplicationSetting _applicationSettings;

        public RouteSegmentFactory(IOptions<ApplicationSetting> settings)
        {
            _applicationSettings = settings.Value;
        }

        public List<RouteSegment> Create(string text)
        {
            var routeSegments = new List<RouteSegment>();

            var lines = new WKTReader().Read(text) as GeometryCollection;
            foreach (var line in lines.Geometries)
            {
                routeSegments.Add(new RouteSegment
                {
                    Mrid = Guid.NewGuid(),
                    Coord = line.AsBinary(),
                    ApplicationName = _applicationSettings.ApplicationName
                });
            }

            return routeSegments;
        }
    }
}
