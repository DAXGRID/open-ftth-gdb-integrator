using System;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenFTTH.GDBIntegrator.RouteNetwork
{
    public class RouteNode
    {
        public Guid Mrid { get; }
        public byte[] Coord { get; }
        public Guid WorkTaskMrid { get; }
        public string Username { get; }
        public string ApplicationName { get; }

        // Default constructor is needed for serialization
        public RouteNode() {}

        public RouteNode(Guid mrid, byte[] coord, Guid workTaskMrid, string username, string applicationName)
        {
            Mrid = mrid;
            Coord = coord;
            WorkTaskMrid = workTaskMrid;
            Username = username;
            ApplicationName = applicationName;
        }

        public virtual string GetGeoJsonCoordinate()
        {
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(Coord);
            var point = (Point)geometry;
            var serializer = GeoJsonSerializer.Create();

            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, point);
                var geoJson = stringWriter.ToString();
                return JObject.Parse(geoJson)["coordinates"].ToString(Formatting.None);
            };
        }
    }
}
