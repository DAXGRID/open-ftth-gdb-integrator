using FluentMigrator;
using System.IO;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1595938168)]
    public class InitialDatabaseSetup : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/Postgres/SchemaMigration/Scripts/create_route_network_schema.sql");
        }

        public override void Down()
        {
            Delete.Schema("route_network");
            Delete.Schema("route_network_integrator");
        }
    }
}
