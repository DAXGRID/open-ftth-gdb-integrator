using System;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.RouteNetwork
{
    public class RouteNode
    {
        public virtual Guid Mrid { get; set; }
        public byte[] Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual string ApplicationInfo { get; set; }
        public virtual bool MarkAsDeleted { get; set; }
        public virtual bool DeleteMe { get; set; }
        public LifecycleInfo LifeCycleInfo { get; set; }
        public MappingInfo MappingInfo { get; set; }
        public SafetyInfo SafetyInfo { get; set; }
        public RouteNodeInfo RouteNodeInfo { get; set; }
        public NamingInfo NamingInfo { get; set; }

        // Default constructor is needed for serialization
        public RouteNode() { }

        public RouteNode(Guid mrid, byte[] coord, Guid workTaskMrid, string username, string applicationName)
        {
            Mrid = mrid;
            Coord = coord;
            WorkTaskMrid = workTaskMrid;
            Username = username;
            ApplicationName = applicationName;
        }

        public RouteNode(Guid mrid, byte[] coord, Guid workTaskMrid, string username, string applicationName, bool markedAsDeleted)
        {
            Mrid = mrid;
            Coord = coord;
            WorkTaskMrid = workTaskMrid;
            Username = username;
            ApplicationName = applicationName;
            MarkAsDeleted = markedAsDeleted;
        }

        public virtual Point GetPoint()
        {
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(Coord);
            return (Point)geometry;
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
