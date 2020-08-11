using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597141930)]
    public class AddSafetyAttributes : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("safety_classification").AsString(255).Nullable()
                .AddColumn("safety_remark").AsString(int.MaxValue).Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("safety_classification").AsString(255).Nullable()
                .AddColumn("safety_remark").AsString(int.MaxValue).Nullable();

            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("safety_classification").AsString(255).Nullable()
                .AddColumn("safety_remark").AsString(int.MaxValue).Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("safety_classification").AsString(255).Nullable()
                .AddColumn("safety_remark").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("safety_classification")
                .Column("safety_remark")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("safety_classification")
                .Column("safety_remark")
                .FromTable("route_segment")
                .InSchema("route_network_integrator");

            Delete
                .Column("safety_classification")
                .Column("safety_remark")
                .FromTable("route_node")
                .InSchema("route_network");
            Delete
                .Column("safety_classification")
                .Column("safety_remark")
                .FromTable("route_node")
                .InSchema("route_network_integrator");
        }
    }
}
