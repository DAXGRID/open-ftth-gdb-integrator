using System;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.QueryModels
{
    public class RouteNodeQueryModel
    {
        public virtual Guid Mrid { get; set; }
        public virtual byte[] Coord { get; set; }
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationInfo { get; set; }
        public bool MarkedToBeDeleted { get; set; }
        public bool DeleteMe { get; set; }

        public string LifeCycleDeploymentState { get; set; }
        public DateTime? LifeCycleInstallationDate { get; set; }
        public DateTime? LifeCycleRemovalDate { get; set; }

        public string MappingMethod { get; set; }
        public string MappingVerticalAccuracy { get; set; }
        public string MappingHorizontalAccuracy { get; set; }
        public string MappingSourceInfo { get; set; }
        public DateTime? MappingSurveyDate { get; set; }

        public string SafetyClassification { get; set; }
        public string SafetyRemark { get; set; }

        public string RouteNodeKind { get; set; }
        public string RouteNodeFunction { get; set; }

        public string NamingName { get; set; }
        public string NamingDescription { get; set; }
    }
}
