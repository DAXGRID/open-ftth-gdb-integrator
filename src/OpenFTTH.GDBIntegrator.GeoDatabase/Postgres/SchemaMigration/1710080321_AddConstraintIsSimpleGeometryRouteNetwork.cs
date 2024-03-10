using FluentMigrator;
using System.IO;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1710080321)]
    public class AddConstraintIsSimpleGeometryRouteNetwork : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/Postgres/SchemaMigration/Scripts/set_constraint_is_simple_route_network.sql");
        }

        public override void Down()
        {
            // This is a hard change to do and needs to be resolved manually if a down is needed to be done.
        }
    }
}
