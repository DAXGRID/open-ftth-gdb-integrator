using System;

namespace OpenFTTH.GDBIntegrator.Model
{
    public class RouteNode
    {
        public Guid Mrid { get; }
        public byte[] Coord { get; }
        public Guid WorkTaskMrid { get; }
        public string Username { get; }
        public string ApplicationName { get; }

        public RouteNode(Guid mrid, byte[] coord, Guid workTaskMrid, string username, string applicationName)
        {
            Mrid = mrid;
            Coord = coord;
            WorkTaskMrid = workTaskMrid;
            Username = username;
            ApplicationName = applicationName;
        }
    }
}
