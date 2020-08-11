using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597143320)]
    public class AddRouteSegmentAttributes : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("routesegment_kind").AsInt32().Nullable()
                .AddColumn("routesegment_width").AsString(255).Nullable()
                .AddColumn("routesegment_height").AsString(255).Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("routesegment_kind").AsString(255).Nullable()
                .AddColumn("routesegment_width").AsString(255).Nullable()
                .AddColumn("routesegment_height").AsString(255).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("routesegment_kind")
                .Column("routesegment_width")
                .Column("routesegment_height")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("routesegment_kind")
                .Column("routesegment_width")
                .Column("routesegment_height")
                .FromTable("route_segment")
                .InSchema("route_network_integrator");
        }
    }
}
