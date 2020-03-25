using System;

namespace OpenFTTH.GDBIntegrator.Model
{
    public class RouteSegment
    {
        public Guid Mrid { get; set; }
        public string Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }
    }
}
