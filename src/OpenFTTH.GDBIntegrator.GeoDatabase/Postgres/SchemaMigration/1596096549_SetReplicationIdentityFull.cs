using FluentMigrator;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1596096549)]
    public class SetReplicationIdentityFull : Migration
    {
        public override void Up()
        {
            Execute.Sql("ALTER TABLE route_network.route_segment REPLICA IDENTITY FULL");
            Execute.Sql("ALTER TABLE route_network.route_node REPLICA IDENTITY FULL");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE route_network.route_segment REPLICA IDENTITY DEFAULT");
            Execute.Sql("ALTER TABLE route_network.route_node REPLICA IDENTITY DEFAULT");
        }
    }
}
