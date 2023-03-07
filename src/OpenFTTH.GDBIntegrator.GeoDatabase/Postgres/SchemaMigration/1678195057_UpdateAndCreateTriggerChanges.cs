using FluentMigrator;
using System.IO;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.SchemaMigration
{
    [Migration(1678195057)]
    public class UpdateAndCreateTriggerChanges : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/Postgres/SchemaMigration/Scripts/update_and_create_trigger_changes.sql");
        }

        public override void Down()
        {
            // This is a hard change to do and needs to be resolved manually if a down is needed to be done.
        }
    }
}
