using System;
using System.Collections.Generic;
using System.Linq;
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
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false
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
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false
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
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_node
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteSegment>(query, new { routeNode.Mrid, _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode, RouteSegment notInclude)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_node
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND mrid != @sMrid AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteSegment>(query, new { sMrid = notInclude.Mrid, routeNode.Mrid, _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodes(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid),
                        @tolerance),
                      coord)
                      AND marked_to_be_deleted = false
                      AND NOT ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance),
                      coord)
                      AND NOT ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance),
                      coord)";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task<List<RouteNode>> GetIntersectingRouteNodes(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        (SELECT coord FROM route_network_integrator.route_node
                        WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND mrid != @mrid AND marked_to_be_deleted = false
                    ";

                await connection.OpenAsync();
                var routeNodes = await connection.QueryAsync<RouteNode>(query, new { routeNode.Mrid, _applicationSettings.Tolerance });

                return routeNodes.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord) AND mrid != @mrid AND marked_to_be_deleted = false
                    ";

                await connection.OpenAsync();
                var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return routeSegments.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid)
                          ),
                        @tolerance
                      ),
                      coord) AND mrid != @mrid AND marked_to_be_deleted = false
                    ";

                await connection.OpenAsync();
                var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

                return routeSegments.AsList();
            }
        }

        public async Task DeleteRouteNode(Guid mrid)
        {
            var applicationName = _applicationSettings.ApplicationName;

            using (var connection = GetNpgsqlConnection())
            {
                var intergratorQuery = @"
                    UPDATE route_network_integrator.route_node
                    SET
                      delete_me = true
                    WHERE mrid = @mrid;
                    ";

                var query = @"
                    UPDATE route_network.route_node
                    SET
                       delete_me = true
                    WHERE mrid = @mrid;
                    ";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, new { mrid });
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task DeleteRouteSegment(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var intergratorQuery = @"
                    UPDATE route_network_integrator.route_segment
                    SET
                      delete_me = true
                    WHERE mrid = @mrid;";

                var query = @"
                    UPDATE route_network.route_segment
                    SET
                      delete_me = true
                    WHERE mrid = @mrid";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, new { mrid });
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task MarkDeleteRouteSegment(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var intergratorQuery = @"
                    UPDATE route_network_integrator.route_segment
                    SET
                      marked_to_be_deleted = true
                    WHERE mrid = @mrid;";

                var query = @"
                    UPDATE route_network.route_segment
                    SET
                      marked_to_be_deleted = true
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, new { mrid });
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task MarkDeleteRouteNode(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var intergratorQuery = @"
                    UPDATE route_network_integrator.route_node
                    SET
                      marked_to_be_deleted = true
                    WHERE mrid = @mrid;";

                var query = @"
                    UPDATE route_network.route_node
                    SET
                      marked_to_be_deleted = true
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, new { mrid });
                await connection.ExecuteAsync(query, new { mrid });
            }
        }

        public async Task InsertRouteNode(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var intergratorQuery = @"
                    INSERT INTO route_network_integrator.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    );";

                var query = @"
                    INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    );";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, routeNode);
                await connection.ExecuteAsync(query, routeNode);
            }
        }

        public async Task InsertRouteNodeIntegrator(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    INSERT INTO route_network_integrator.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    ) ON CONFLICT ON CONSTRAINT route_node_pkey DO NOTHING;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeNode);
            }
        }

        public async Task InsertRouteSegmentIntegrator(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    INSERT INTO route_network_integrator.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    ) ON CONFLICT ON CONSTRAINT route_segment_pkey DO NOTHING;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeSegment);
            }
        }

        public async Task UpdateRouteSegmentIntegrator(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    UPDATE route_network_integrator.route_segment
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeSegment);
            }
        }

        public async Task UpdateRouteNodeIntegrator(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    UPDATE route_network_integrator.route_node
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, routeNode);
            }
        }

        public async Task InsertRouteSegment(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var integratorQuery = @"
                    INSERT INTO route_network_integrator.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    );";

                var query = @"
                    INSERT INTO route_network.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false
                    );";

                await connection.OpenAsync();
                await connection.ExecuteAsync(integratorQuery, routeSegment);
                await connection.ExecuteAsync(query, routeSegment);
            }
        }

        public async Task<string> GetRouteSegmentsSplittedByRouteNode(RouteNode routeNode, RouteSegment intersectingRouteSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    SELECT ST_AsText(
                        ST_Split(
                            ST_Snap(
                                route_segment.coord,
                                ST_GeomFromWKB(@coord, 25832),
                                0.02
                            ),
                            ST_GeomFromWKB(@coord, 25832)
                        )
                    )
                    FROM route_network_integrator.route_segment WHERE mrid = @mrid AND marked_to_be_deleted = false;
                ";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<string>(query, new { routeNode.Coord, intersectingRouteSegment.Mrid });

                return result.First();
            }
        }

        private NpgsqlConnection GetNpgsqlConnection()
        {
            return new NpgsqlConnection(
                        $"Host={_postgisSettings.Host};Port={_postgisSettings.Port};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}");
        }
    }
}
