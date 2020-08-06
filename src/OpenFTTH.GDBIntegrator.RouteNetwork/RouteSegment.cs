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
        public virtual Guid Mrid { get; set; }
        public virtual byte[] Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationInfo { get; set; }
        public virtual bool MarkAsDeleted { get; set; }
        public virtual bool DeleteMe { get; set; }
        public string SegmentKind { get; set; }

        public virtual Point FindStartPoint()
        {
            var lineString = GetLineString();

            var startPoint = new Point(lineString.StartPoint.X, lineString.StartPoint.Y);

            return startPoint;
        }

        public virtual Point FindEndPoint()
        {
            var lineString = GetLineString();

            var endPoint = new Point(lineString.EndPoint.X, lineString.EndPoint.Y);

            return endPoint;
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

        public virtual LineString GetLineString()
        {
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(Coord);
            var lineString = (LineString)geometry;

            return lineString;
        }
    }
}
