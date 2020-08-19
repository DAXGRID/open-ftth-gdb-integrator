using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597139805)]
    public class AddMappingAttributes : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("mapping_method").AsString(255).Nullable()
                .AddColumn("mapping_vertical_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_horizontal_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_source_info").AsString(int.MaxValue).Nullable()
                .AddColumn("mapping_survey_date").AsDate().Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("mapping_method").AsString(255).Nullable()
                .AddColumn("mapping_vertical_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_horizontal_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_source_info").AsString(int.MaxValue).Nullable()
                .AddColumn("mapping_survey_date").AsDate().Nullable();

            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("mapping_method").AsString(255).Nullable()
                .AddColumn("mapping_vertical_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_horizontal_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_source_info").AsString(int.MaxValue).Nullable()
                .AddColumn("mapping_survey_date").AsDate().Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("mapping_method").AsString(255).Nullable()
                .AddColumn("mapping_vertical_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_horizontal_accuracy").AsString(255).Nullable()
                .AddColumn("mapping_source_info").AsString(int.MaxValue).Nullable()
                .AddColumn("mapping_survey_date").AsDate().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("mapping_method")
                .Column("mapping_vertical_accuracy")
                .Column("mapping_horizontal_accuracy")
                .Column("mapping_source_info")
                .Column("mapping_survey_date")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("mapping_method")
                .Column("mapping_vertical_accuracy")
                .Column("mapping_horizontal_accuracy")
                .Column("mapping_source_info")
                .Column("mapping_survey_date")
                .FromTable("route_segment")
                .InSchema("route_network_integrator");

            Delete
                .Column("mapping_method")
                .Column("mapping_vertical_accuracy")
                .Column("mapping_horizontal_accuracy")
                .Column("mapping_source_info")
                .Column("mapping_survey_date")
                .FromTable("route_node")
                .InSchema("route_network");

            Delete
                .Column("mapping_method")
                .Column("mapping_vertical_accuracy")
                .Column("mapping_horizontal_accuracy")
                .Column("mapping_source_info")
                .Column("mapping_survey_date")
                .FromTable("route_node")
                .InSchema("route_network_integrator");
        }
    }
}
