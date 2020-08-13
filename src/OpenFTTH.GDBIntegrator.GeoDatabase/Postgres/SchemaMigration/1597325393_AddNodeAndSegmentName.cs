using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597325393)]
    public class AddNodeAndSegmentName : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("naming_name").AsString(255).Nullable()
                .AddColumn("naming_description").AsString(2048).Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("naming_name").AsString(255).Nullable()
                .AddColumn("naming_description").AsString(2048).Nullable();

            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("naming_name").AsString(255).Nullable()
                .AddColumn("naming_description").AsString(2048).Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("naming_name").AsString(255).Nullable()
                .AddColumn("naming_description").AsString(2048).Nullable();

        }

        public override void Down()
        {
            Delete
                .Column("segment_kind")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("segment_kind")
                .FromTable("route_segment")
                .InSchema("route_network_integrator");
        }
    }
}
