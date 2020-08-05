using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1596653853)]
    public class MarkedToBeDeletedAndDeleteMeNotNullable : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable();

            Alter.Table("route_segment").InSchema("route_network").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            Alter.Table("route_segment").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().Nullable();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().Nullable();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().Nullable();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().Nullable();

            Alter.Table("route_segment").InSchema("route_network").AlterColumn("delete_me").AsBoolean().Nullable();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("delete_me").AsBoolean().Nullable();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().Nullable();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().Nullable();
        }
    }
}
