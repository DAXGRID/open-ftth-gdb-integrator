using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1596653853)]
    public class MarkedToBeDeletedAndDeleteMeNotNullable : Migration
    {
        public override void Up()
        {
            Alter.Table("route_segment").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable().Indexed();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable().Indexed();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable().Indexed();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("marked_to_be_deleted").AsBoolean().NotNullable().Indexed();

            Alter.Table("route_segment").InSchema("route_network").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_segment").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().NotNullable();
            Alter.Table("route_node").InSchema("route_network_integrator").AlterColumn("delete_me").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            Delete.Column("marked_to_be_deleted").FromTable("route_segment").InSchema("route_network");
            Delete.Column("marked_to_be_deleted").FromTable("route_segment").InSchema("route_network_integrator");
            Delete.Column("marked_to_be_deleted").FromTable("route_node").InSchema("route_network");
            Delete.Column("marked_to_be_deleted").FromTable("route_node").InSchema("route_network_integrator");

            Delete.Column("delete_me").FromTable("route_segment").InSchema("route_network");
            Delete.Column("delete_me").FromTable("route_segment").InSchema("route_network_integrator");
            Delete.Column("delete_me").FromTable("route_node").InSchema("route_network");
            Delete.Column("delete_me").FromTable("route_node").InSchema("route_network_integrator");
        }
    }
}
