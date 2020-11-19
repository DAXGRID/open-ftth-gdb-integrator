using System.IO;
using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1605796532)]
    public class DdlSurveyImport : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/Postgres/SchemaMigration/Scripts/ddl_survey_import.sql");
        }

        public override void Down()
        {
            Delete.Index("INDEX idx_route_node_work_task_mrid").OnTable("route_node").InSchema("route_network");
            Delete.Index("INDEX idx_route_segment_work_task_mrid").OnTable("route_segment").InSchema("route_network");

            Delete
                .Column("lifecycle_documentation_state")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("lifecycle_documentation_state")
                .FromTable("route_node")
                .InSchema("route_network");

            Execute.Sql("DROP VIEW route_network.route_segment_survey_import, route_network.route_node_survey_import");
        }
    }
}
