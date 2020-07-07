using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace OpenFTTH.GDBIntegrator.RouteNetwork
{
    public class RouteSegment
    {
        public Guid Mrid { get; set; }
        public string Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }

        public RouteNode FindStartNode()
        {
            var routeSegmentBytes = Convert.FromBase64String(Coord);
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(routeSegmentBytes);
            var lineString = (LineString)geometry;

            var wkbWriter = new WKBWriter();

            var startPoint = new Point(lineString.StartPoint.X, lineString.StartPoint.Y);
            var startNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(startPoint), Guid.NewGuid(), String.Empty, String.Empty);

            return startNode;
        }

        public RouteNode FindEndNode()
        {
            var routeSegmentBytes = Convert.FromBase64String(Coord);
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(routeSegmentBytes);
            var lineString = (LineString)geometry;

            var wkbWriter = new WKBWriter();

            var endPoint = new Point(lineString.EndPoint.X, lineString.EndPoint.Y);
            var endNode = new RouteNode(Guid.NewGuid(), wkbWriter.Write(endPoint), Guid.NewGuid(), String.Empty, String.Empty);

            return endNode;
        }
    }
}
