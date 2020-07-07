using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres
{
    public class Postgis : IGeoDatabase
    {
        private readonly PostgisSetting _postgisSettings;

        public Postgis(IOptions<PostgisSetting> postgisSetting)
        {
            _postgisSettings = postgisSetting.Value;
        }

        public async Task<List<RouteNode>> GetIntersectingRouteNodes(RouteSegment routeSegment)
        {
            using (var connection = new NpgsqlConnection(
                       $"Host={_postgisSettings.Host};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}"))
            {
                var query = $@"INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName
                    );";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeSegment);

                return new List<RouteNode>();
            }
        }

        public async Task InsertRouteNode(RouteNode routeNode)
        {
           using (var connection = new NpgsqlConnection(
                       $"Host={_postgisSettings.Host};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}"))
            {
                var query = $@"INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName
                    );";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeNode);
            }
        }
    }
}
