using System;
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
        private readonly ApplicationSetting _applicationSettings;

        public Postgis(IOptions<PostgisSetting> postgisSettings, IOptions<ApplicationSetting> applicationSettings)
        {
            _postgisSettings = postgisSettings.Value;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task<List<RouteNode>> GetIntersectingStartRouteNodes(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = $@"SELECT (ST_AsText(coord), mrid) FROM route_network.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                          (SELECT coord FROM route_network.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord)
                    ";

                await connection.OpenAsync();

                var routeNodes = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return routeNodes.AsList();
            }
        }

        public async Task<List<RouteNode>> GetIntersectingEndRouteNodes(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = $@"SELECT (ST_AsText(coord), mrid) FROM route_network.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                          (SELECT coord FROM route_network.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord)
                    ";

                await connection.OpenAsync();

                var routeNodes = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return routeNodes.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = $@"SELECT (ST_AsText(coord), mrid) FROM route_network.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network.route_node
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord)";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteSegment>(query, new { routeNode.Mrid, _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task DeleteRouteNode(Guid mrid)
        {
            var applicationName = _applicationSettings.ApplicationName;

            using (var connection = GetNpgsqlConnection())
            {
                var query = @"UPDATE route_network.route_node
                    SET
                      delete_me = true
                    WHERE mrid = @mrid";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task DeleteRouteSegment(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"UPDATE route_network.route_segment
                    SET
                      delete_me = true
                    WHERE mrid = @mrid";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task InsertRouteNode(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
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

        private async Task InsertRouteSegment(RouteSegment routeSegment)
        {
           using (var connection = GetNpgsqlConnection())
            {
                var query = $@"INSERT INTO route_network.route_segment(
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
            }
        }

        private NpgsqlConnection GetNpgsqlConnection()
        {
            return new NpgsqlConnection(
                        $"Host={_postgisSettings.Host};Port={_postgisSettings.Port};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}");
        }
    }
}
