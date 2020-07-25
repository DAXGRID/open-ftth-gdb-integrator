using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Factories
{
    public class RouteNodeFactory : IRouteNodeFactory
    {
        private ApplicationSetting _applicationSettings;

        public RouteNodeFactory(IOptions<ApplicationSetting> applicationSettings)
        {
            _applicationSettings = applicationSettings.Value;
        }

        public RouteNode Create(Point point)
        {
            var wkbWriter = new WKBWriter();

            var node = new RouteNode
                (
                    Guid.NewGuid(),
                    wkbWriter.Write(point),
                    Guid.NewGuid(),
                    _applicationSettings.ApplicationName,
                    _applicationSettings.ApplicationName
                );

            return node;
        }
    }
}
