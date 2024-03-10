using FluentMigrator;
using System.IO;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1710077940)]
    public class AddConstraintNotNullGeometryRouteNetwork : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/Postgres/SchemaMigration/Scripts/set_not_null_constraint_route_network.sql");
        }

        public override void Down()
        {
            // This is a hard change to do and needs to be resolved manually if a down is needed to be done.
        }
    }
}
