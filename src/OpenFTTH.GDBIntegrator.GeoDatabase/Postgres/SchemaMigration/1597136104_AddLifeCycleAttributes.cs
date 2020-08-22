using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1597136104)]
    public class AddLifeCycleAttributes : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network")
                .AddColumn("lifecycle_deployment_state").AsString(255).Nullable()
                .AddColumn("lifecycle_installation_date").AsDateTimeOffset().Nullable()
                .AddColumn("lifecycle_removal_date").AsDateTimeOffset().Nullable();

            Alter.Table("route_segment").InSchema("route_network_integrator")
                .AddColumn("lifecycle_deployment_state").AsString(255).Nullable()
                .AddColumn("lifecycle_installation_date").AsDateTimeOffset().Nullable()
                .AddColumn("lifecycle_removal_date").AsDateTimeOffset().Nullable();

            Alter.Table("route_node").InSchema("route_network")
                .AddColumn("lifecycle_deployment_state").AsString(255).Nullable()
                .AddColumn("lifecycle_installation_date").AsDateTimeOffset().Nullable()
                .AddColumn("lifecycle_removal_date").AsDateTimeOffset().Nullable();

            Alter.Table("route_node").InSchema("route_network_integrator")
                .AddColumn("lifecycle_deployment_state").AsString(255).Nullable()
                .AddColumn("lifecycle_installation_date").AsDateTimeOffset().Nullable()
                .AddColumn("lifecycle_removal_date").AsDateTimeOffset().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("lifecycle_deployment_state")
                .Column("lifecycle_installation_date")
                .Column("lifecycle_removal_date")
                .FromTable("route_segment")
                .InSchema("route_network");

            Delete
                .Column("lifecycle_deployment_state")
                .Column("lifecycle_installation_date")
                .Column("lifecycle_removal_date")
                .FromTable("route_segment")
                .InSchema("route_network_integrator");

            Delete
                .Column("lifecycle_deployment_state")
                .Column("lifecycle_installation_date")
                .Column("lifecycle_removal_date")
                .FromTable("route_node")
                .InSchema("route_network");

            Delete
                .Column("lifecycle_deployment_state")
                .Column("lifecycle_installation_date")
                .Column("lifecycle_removal_date")
                .FromTable("route_node")
                .InSchema("route_network_integrator");
        }
    }
}
