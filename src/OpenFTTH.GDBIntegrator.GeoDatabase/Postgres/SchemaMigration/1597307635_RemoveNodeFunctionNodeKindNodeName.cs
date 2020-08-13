using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597307635)]
    public class RemoveNodeFunctionNodeKindNodeName : Migration
    {
        public override void Up()
        {
            Delete
                .Column("node_kind")
                .Column("node_function")
                .Column("node_name")
                .FromTable("route_node")
                .InSchema("route_network");

            Delete
                .Column("node_kind")
                .Column("node_function")
                .Column("node_name")
                .FromTable("route_node")
                .InSchema("route_network_integrator");
        }

        public override void Down()
        {
            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("node_kind").AsString(255).Nullable()
                .AddColumn("node_function").AsString(255).Nullable()
                .AddColumn("node_name").AsString(255).Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("node_kind").AsString(255).Nullable()
                .AddColumn("node_function").AsString(255).Nullable()
                .AddColumn("node_name").AsString(255).Nullable();
        }
    }
}
