using System;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenFTTH.GDBIntegrator.RouteNetwork
{
    public class RouteSegment
    {
        public Guid Mrid { get; set; }
        public byte[] Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }

        public virtual bool IsValidAndSimple()
        {
            var lineString = GetLineString();
            return lineString.IsValid && lineString.IsSimple;
        }

        public virtual RouteNode FindStartNode()
        {
            var lineString = GetLineString();

            var startPoint = new Point(lineString.StartPoint.X, lineString.StartPoint.Y);
            var wkbWriter = new WKBWriter();

            var startNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(startPoint), Guid.NewGuid(), "GDB_INTEGRATOR", "GDB_INTEGRATOR");

            return startNode;
        }

        public virtual RouteNode FindEndNode()
        {
            var lineString = GetLineString();
            var wkbWriter = new WKBWriter();

            var endPoint = new Point(lineString.EndPoint.X, lineString.EndPoint.Y);
            var endNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(endPoint), Guid.NewGuid(), "GDB_INTEGRATOR", "GDB_INTEGRATOR");

            return endNode;
        }

        public virtual string GetGeoJsonCoordinate()
        {
            var lineString = GetLineString();
            var serializer = GeoJsonSerializer.Create();

            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, lineString);
                var geoJson = stringWriter.ToString();
                return JObject.Parse(geoJson)["coordinates"].ToString(Formatting.None);
            };
        }

        private LineString GetLineString()
        {
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(Coord);
            var lineString = (LineString)geometry;

            return lineString;
        }
    }
}
