using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597308110)]
    public class RemoveSegmentKind : Migration
    {
        public override void Up()
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

        public override void Down()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("segment_kind").AsString(255).Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("segment_kind").AsString(255).Nullable();
        }
    }
}
