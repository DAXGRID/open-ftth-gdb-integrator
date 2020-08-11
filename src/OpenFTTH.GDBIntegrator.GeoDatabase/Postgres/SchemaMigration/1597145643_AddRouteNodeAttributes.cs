using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597145643)]
    public class AddRouteNodeAttributes : Migration
    {
        public override void Up()
        {
            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("routenode_kind").AsInt32().Nullable()
                .AddColumn("routenode_function").AsInt32().Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("routenode_kind").AsInt32().Nullable()
                .AddColumn("routenode_function").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("routenode_kind")
                .Column("routenode_function")
                .FromTable("route_node")
                .InSchema("route_network");

            Delete
                .Column("routenode_kind")
                .Column("routenode_function")
                .FromTable("route_node")
                .InSchema("route_network_integrator");
        }
    }
}
