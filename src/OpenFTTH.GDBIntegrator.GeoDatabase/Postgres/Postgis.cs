using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.QueryModels;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres
{
    public class Postgis : IGeoDatabase
    {
        private readonly PostgisSetting _postgisSettings;
        private readonly ApplicationSetting _applicationSettings;
        private readonly IInfoMapper _infoMapper;

        public Postgis(IOptions<PostgisSetting> postgisSettings, IOptions<ApplicationSetting> applicationSettings, IInfoMapper infoMapper)
        {
            _postgisSettings = postgisSettings.Value;
            _applicationSettings = applicationSettings.Value;
            _infoMapper = infoMapper;
        }

        public async Task<RouteNode> GetRouteNodeShadowTable(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT
                              ST_AsBinary(coord) AS coord,
                              mrid,
                              marked_to_be_deleted AS markedToBeDeleted,
                              work_task_mrid AS workTaskMrid,
                              user_name AS userName,
                              application_name AS applicationName
                              FROM route_network_integrator.route_node WHERE mrid = @mrid AND marked_to_be_deleted = false";

                await connection.OpenAsync();

                var routeNode = await connection.QueryAsync<RouteNode>(query, new { mrid });

                return routeNode.FirstOrDefault();
            }
        }

        public async Task<RouteSegment> GetRouteSegmentShadowTable(Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT
                              ST_AsBinary(coord) AS coord,
                              mrid,
                              marked_to_be_deleted AS markedToBeDeleted,
                              work_task_mrid AS workTaskMrid,
                              user_name AS userName,
                              application_name AS applicationName
                              FROM route_network_integrator.route_segment WHERE mrid = @mrid AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var routeSegment = await connection.QueryAsync<RouteSegment>(query, new { mrid });

                return routeSegment.FirstOrDefault();
            }
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

        public async Task<List<RouteNode>> GetIntersectingStartRouteNodes(byte[] coord)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                            ST_GeomFromWKB(@coord, 25832)
                          ),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false
                    ";

                await connection.OpenAsync();
                var routeNodes = await connection.QueryAsync<RouteNode>(query, new { coord, _applicationSettings.Tolerance });

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

        public async Task<List<RouteNode>> GetIntersectingEndRouteNodes(byte[] coord)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                            ST_GeomFromWKB(@coord, 25832)
                          ),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false
                    ";

                await connection.OpenAsync();
                var routeNodes = await connection.QueryAsync<RouteNode>(query, new { coord, _applicationSettings.Tolerance });

                return routeNodes.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT
                    ST_AsBinary(coord) AS coord,
                    mrid,
                    work_task_mrid AS workTaskMrid,
                    user_name AS username,
                    application_name AS applicationName,
                    application_info AS applicationInfo,
                    mapping_method AS mappingMethod,
                    mapping_vertical_accuracy AS mappingVerticalAccuracy,
                    mapping_horizontal_accuracy AS mappingHorizontalAccuracy,
                    mapping_source_info AS mappingSourceInfo,
                    mapping_survey_date AS mappingSurveyDate,
                    lifecycle_deployment_state AS lifeCycleDeploymentState,
                    lifecycle_installation_date AS lifeCycleInstallationDate,
                    lifecycle_removal_date AS lifeCycleRemovalDate,
                    naming_name AS namingName,
                    naming_description AS namingDescription,
                    routesegment_height AS routeSegmentHeight,
                    routeSegment_kind AS routeSegmentKind,
                    routesegment_width AS routeSegmentWidth,
                    safety_classification AS safetyClassification,
                    safety_remark AS safetyRemark
                    FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_node
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var routeSegments = (await connection.QueryAsync<RouteSegmentQueryModel>(query, new { routeNode.Mrid, _applicationSettings.Tolerance }))
                    .Select(x => new RouteSegment
                    {
                        ApplicationInfo = x.ApplicationInfo,
                        ApplicationName = x.ApplicationName,
                        Coord = x.Coord,
                        Mrid = x.Mrid,
                        Username = x.Username,
                        WorkTaskMrid = x.WorkTaskMrid,
                        MappingInfo = new MappingInfo
                        {
                            HorizontalAccuracy = x.MappingHoritzontalAccuracy,
                            Method = _infoMapper.MapMappingMethod(x.MappingMethod),
                            SourceInfo = x.MappingSourceInfo,
                            SurveyDate = x.MappingSurveyDate,
                            VerticalAccuracy = x.MappingVerticalAccuracy
                        },
                        LifeCycleInfo = new LifecycleInfo
                        {
                            DeploymentState = _infoMapper.MapDeploymentState(x.LifeCycleDeploymentState),
                            InstallationDate = x.LifeCycleInstallationDate,
                            RemovalDate = x.LifeCycleRemovalDate
                        },
                        NamingInfo = new NamingInfo
                        {
                            Description = x.NamingDescription,
                            Name = x.NamingName
                        },
                        RouteSegmentInfo = new RouteSegmentInfo
                        {
                            Height = x.RouteSegmentHeight,
                            Kind = _infoMapper.MapRouteSegmentKind(x.RouteSegmentKind),
                            Width = x.RouteSegmentWidth
                        },
                        SafetyInfo = new SafetyInfo
                        {
                            Classification = x.SafetyClassification,
                            Remark = x.SafetyRemark
                        }
                    }).ToList();

                foreach (var routeSegment in routeSegments)
                {
                    // Make fully empty objects into nulls.
                    routeSegment.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(routeSegment.LifeCycleInfo) ? routeSegment.LifeCycleInfo : null;
                    routeSegment.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(routeSegment.MappingInfo) ? routeSegment.MappingInfo : null;
                    routeSegment.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(routeSegment.NamingInfo) ? routeSegment.NamingInfo : null;
                    routeSegment.RouteSegmentInfo = AreAnyPropertiesNotNull<RouteSegmentInfo>(routeSegment.RouteSegmentInfo) ? routeSegment.RouteSegmentInfo : null;
                    routeSegment.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(routeSegment.SafetyInfo) ? routeSegment.SafetyInfo : null;
                }

                return routeSegments;
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(byte[] coordinates)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_GeomFromWKB(@coordinates, 25832),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteSegment>(query, new { coordinates, tolerance = _applicationSettings.Tolerance });

                return result.AsList();
            }
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode, RouteSegment notInclude)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"SELECT
                    ST_AsBinary(coord) AS coord,
                    mrid,
                    work_task_mrid AS workTaskMrid,
                    user_name AS username,
                    application_name AS applicationName,
                    application_info AS applicationInfo,
                    mapping_method AS mappingMethod,
                    mapping_vertical_accuracy AS mappingVerticalAccuracy,
                    mapping_horizontal_accuracy AS mappingHorizontalAccuracy,
                    mapping_source_info AS mappingSourceInfo,
                    mapping_survey_date AS mappingSurveyDate,
                    lifecycle_deployment_state AS lifeCycleDeploymentState,
                    lifecycle_installation_date AS lifeCycleInstallationDate,
                    lifecycle_removal_date AS lifeCycleRemovalDate,
                    naming_name AS namingName,
                    naming_description AS namingDescription,
                    routesegment_height AS routeSegmentHeight,
                    routeSegment_kind AS routeSegmentKind,
                    routesegment_width AS routeSegmentWidth,
                    safety_classification AS safetyClassification,
                    safety_remark AS safetyRemark
                    FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_node
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false AND mrid != @sMrid";

                await connection.OpenAsync();
                var routeSegments = (await connection.QueryAsync<RouteSegmentQueryModel>(query, new { sMrid = notInclude.Mrid, routeNode.Mrid, _applicationSettings.Tolerance }))
                    .Select(x => new RouteSegment
                    {
                        ApplicationInfo = x.ApplicationInfo,
                        ApplicationName = x.ApplicationName,
                        Coord = x.Coord,
                        Mrid = x.Mrid,
                        Username = x.Username,
                        WorkTaskMrid = x.WorkTaskMrid,
                        MappingInfo = new MappingInfo
                        {
                            HorizontalAccuracy = x.MappingHoritzontalAccuracy,
                            Method = _infoMapper.MapMappingMethod(x.MappingMethod),
                            SourceInfo = x.MappingSourceInfo,
                            SurveyDate = x.MappingSurveyDate,
                            VerticalAccuracy = x.MappingVerticalAccuracy
                        },
                        LifeCycleInfo = new LifecycleInfo
                        {
                            DeploymentState = _infoMapper.MapDeploymentState(x.LifeCycleDeploymentState),
                            InstallationDate = x.LifeCycleInstallationDate,
                            RemovalDate = x.LifeCycleRemovalDate
                        },
                        NamingInfo = new NamingInfo
                        {
                            Description = x.NamingDescription,
                            Name = x.NamingName
                        },
                        RouteSegmentInfo = new RouteSegmentInfo
                        {
                            Height = x.RouteSegmentHeight,
                            Kind = _infoMapper.MapRouteSegmentKind(x.RouteSegmentKind),
                            Width = x.RouteSegmentWidth
                        },
                        SafetyInfo = new SafetyInfo
                        {
                            Classification = x.SafetyClassification,
                            Remark = x.SafetyRemark
                        }
                    }).ToList();

                foreach (var routeSegment in routeSegments)
                {
                    // Make fully empty objects into nulls.
                    routeSegment.LifeCycleInfo = AreAnyPropertiesNotNull<LifecycleInfo>(routeSegment.LifeCycleInfo) ? routeSegment.LifeCycleInfo : null;
                    routeSegment.MappingInfo = AreAnyPropertiesNotNull<MappingInfo>(routeSegment.MappingInfo) ? routeSegment.MappingInfo : null;
                    routeSegment.NamingInfo = AreAnyPropertiesNotNull<NamingInfo>(routeSegment.NamingInfo) ? routeSegment.NamingInfo : null;
                    routeSegment.RouteSegmentInfo = AreAnyPropertiesNotNull<RouteSegmentInfo>(routeSegment.RouteSegmentInfo) ? routeSegment.RouteSegmentInfo : null;
                    routeSegment.SafetyInfo = AreAnyPropertiesNotNull<SafetyInfo>(routeSegment.SafetyInfo) ? routeSegment.SafetyInfo : null;
                }

                return routeSegments;
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

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(byte[] coordinates, Guid mrid)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var query = @"
                    SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_GeomFromWKB(@coordinates, 25832),
                        @tolerance),
                      coord)
                      AND marked_to_be_deleted = false
                      AND NOT ST_Intersects(
                      ST_Buffer(
                        ST_StartPoint(
                          ST_GeomFromWKB(@coordinates, 25832)),
                          @tolerance),
                      coord)
                      AND NOT ST_Intersects(
                      ST_Buffer(
                        ST_EndPoint(
                          ST_GeomFromWKB(@coordinates, 25832)),
                          @tolerance),
                      coord)";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<RouteNode>(query, new { mrid, coordinates, _applicationSettings.Tolerance });

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
                    marked_to_be_deleted,
                    delete_me
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false,
                    false
                    );";

                var query = @"
                    INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted,
                    delete_me
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    false,
                    false
                    );";

                await connection.OpenAsync();
                await connection.ExecuteAsync(intergratorQuery, routeNode);
                await connection.ExecuteAsync(query, routeNode);
            }
        }

        public async Task UpdateRouteNode(RouteNode routeNode)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var integratorQuery = @"
                    UPDATE route_network_integrator.route_node
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                var query = @"
                    UPDATE route_network.route_node
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(integratorQuery, routeNode);
                await connection.ExecuteAsync(query, routeNode);
            }
        }

        public async Task UpdateRouteNodeShadowTable(RouteNode routeNode)
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
                      application_info = @applicationInfo,
                      marked_to_be_deleted = @markAsDeleted,
                      delete_me = @deleteMe,
                      lifecycle_deployment_state = @lifeCycleDeploymentState,
                      lifecycle_installation_date = @lifeCycleInstallationDate,
                      lifecycle_removal_date = @lifeCycleRemovalDate,
                      mapping_method = @mappingMethod,
                      mapping_vertical_accuracy = @mappingVerticalAccuracy,
                      mapping_horizontal_accuracy = @mappingHorizontalAccuracy,
                      mapping_source_info = @mappingSourceInfo,
                      mapping_survey_date = @mappingSurveyDate,
                      safety_classification = @safetyClassification,
                      safety_remark = @safetyRemark,
                      routenode_kind = @routeNodeKind,
                      routenode_function = @routeNodeFunction,
                      naming_name = @namingName,
                      naming_description = @namingDescription
                    WHERE mrid = @mrid;";

                var mappedRouteNode = new
                {
                    mrid = routeNode.Mrid,
                    coord = routeNode.Coord,
                    workTaskMrId = routeNode.WorkTaskMrid,
                    userName = routeNode.Username,
                    applicationName = routeNode.ApplicationName,
                    applicationInfo = routeNode.ApplicationInfo,
                    markAsDeleted = routeNode.MarkAsDeleted,
                    deleteMe = routeNode.DeleteMe,
                    lifeCycleDeploymentState = routeNode.LifeCycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeNode.LifeCycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeNode.LifeCycleInfo?.RemovalDate,
                    mappingMethod = routeNode.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeNode.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeNode.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeNode.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeNode.MappingInfo?.SurveyDate,
                    safetyClassification = routeNode.SafetyInfo?.Classification,
                    safetyRemark = routeNode.SafetyInfo?.Remark,
                    routeNodeKind = routeNode.RouteNodeInfo?.Kind?.ToString("g"),
                    routeNodeFunction = routeNode.RouteNodeInfo?.Function?.ToString("g"),
                    namingName = routeNode.NamingInfo?.Name,
                    namingDescription = routeNode.NamingInfo?.Description
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, mappedRouteNode);
            }
        }

        public async Task InsertRouteNodeShadowTable(RouteNode routeNode)
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
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routenode_kind,
                    routenode_function,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeNodeKind,
                    @routeNodeFunction,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_node_pkey DO NOTHING;";

                var mappedRouteNode = new
                {
                    mrid = routeNode.Mrid,
                    coord = routeNode.Coord,
                    workTaskMrId = routeNode.WorkTaskMrid,
                    userName = routeNode.Username,
                    applicationName = routeNode.ApplicationName,
                    applicationInfo = routeNode.ApplicationInfo,
                    markAsDeleted = routeNode.MarkAsDeleted,
                    deleteMe = routeNode.DeleteMe,
                    lifeCycleDeploymentState = routeNode.LifeCycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeNode.LifeCycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeNode.LifeCycleInfo?.RemovalDate,
                    mappingMethod = routeNode.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeNode.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeNode.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeNode.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeNode.MappingInfo?.SurveyDate,
                    safetyClassification = routeNode.SafetyInfo?.Classification,
                    safetyRemark = routeNode.SafetyInfo?.Remark,
                    routeNodeKind = routeNode.RouteNodeInfo?.Kind?.ToString("g"),
                    routeNodeFunction = routeNode.RouteNodeInfo?.Function?.ToString("g"),
                    namingName = routeNode.NamingInfo?.Name,
                    namingDescription = routeNode.NamingInfo?.Description
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, mappedRouteNode);
            }
        }

        public async Task InsertRouteSegmentShadowTable(RouteSegment routeSegment)
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
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routesegment_kind,
                    routesegment_height,
                    routesegment_width,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeSegmentKind,
                    @routeSegmentHeight,
                    @routeSegmentWidth,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_segment_pkey DO NOTHING;";

                var mappedRouteSegment = new
                {
                    mrid = routeSegment.Mrid,
                    coord = routeSegment.Coord,
                    workTaskMrId = routeSegment.WorkTaskMrid,
                    userName = routeSegment.Username,
                    applicationName = routeSegment.ApplicationName,
                    applicationInfo = routeSegment.ApplicationInfo,
                    markAsDeleted = routeSegment.MarkAsDeleted,
                    deleteMe = routeSegment.DeleteMe,
                    lifeCycleDeploymentState = routeSegment.LifeCycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeSegment.LifeCycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeSegment.LifeCycleInfo?.RemovalDate,
                    mappingMethod = routeSegment.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeSegment.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeSegment.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeSegment.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeSegment.MappingInfo?.SurveyDate,
                    safetyClassification = routeSegment.SafetyInfo?.Classification,
                    safetyRemark = routeSegment.SafetyInfo?.Remark,
                    routeSegmentKind = routeSegment?.RouteSegmentInfo?.Kind?.ToString("g"),
                    routeSegmentHeight = routeSegment.RouteSegmentInfo?.Height,
                    routeSegmentWidth = routeSegment.RouteSegmentInfo?.Width,
                    namingName = routeSegment.NamingInfo?.Name,
                    namingDescription = routeSegment.NamingInfo?.Description
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, mappedRouteSegment);
            }
        }

        public async Task UpdateRouteSegmentShadowTable(RouteSegment routeSegment)
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
                      marked_to_be_deleted = @markAsDeleted,
                      delete_me = @deleteMe,
                      lifecycle_deployment_state = @lifeCycleDeploymentState,
                      lifecycle_installation_date = @lifeCycleInstallationDate,
                      lifecycle_removal_date = @lifeCycleRemovalDate,
                      mapping_method = @mappingMethod,
                      mapping_vertical_accuracy = @mappingVerticalAccuracy,
                      mapping_horizontal_accuracy = @mappingHorizontalAccuracy,
                      mapping_source_info = @mappingSourceInfo,
                      mapping_survey_date = @mappingSurveyDate,
                      safety_classification = @safetyClassification,
                      safety_remark = @safetyRemark,
                      routesegment_kind = @routeSegmentKind,
                      routesegment_height = @routeSegmentHeight,
                      routeSegment_width = @routeSegmentWidth,
                      naming_name = @namingName,
                      naming_description = @namingDescription
                    WHERE mrid = @mrid;";

                var mappedRouteSegment = new
                {
                    mrid = routeSegment.Mrid,
                    coord = routeSegment.Coord,
                    workTaskMrId = routeSegment.WorkTaskMrid,
                    userName = routeSegment.Username,
                    applicationName = routeSegment.ApplicationName,
                    applicationInfo = routeSegment.ApplicationInfo,
                    markAsDeleted = routeSegment.MarkAsDeleted,
                    deleteMe = routeSegment.DeleteMe,
                    lifeCycleDeploymentState = routeSegment.LifeCycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeSegment.LifeCycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeSegment.LifeCycleInfo?.RemovalDate,
                    mappingMethod = routeSegment.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeSegment.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeSegment.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeSegment.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeSegment.MappingInfo?.SurveyDate,
                    safetyClassification = routeSegment.SafetyInfo?.Classification,
                    safetyRemark = routeSegment.SafetyInfo?.Remark,
                    routeSegmentKind = routeSegment?.RouteSegmentInfo?.Kind?.ToString("g"),
                    routeSegmentHeight = routeSegment.RouteSegmentInfo?.Height,
                    routeSegmentWidth = routeSegment.RouteSegmentInfo?.Width,
                    namingName = routeSegment.NamingInfo?.Name,
                    namingDescription = routeSegment.NamingInfo?.Description
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(query, mappedRouteSegment);
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
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routesegment_kind,
                    routesegment_height,
                    routesegment_width,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeSegmentKind,
                    @routeSegmentHeight,
                    @routeSegmentWidth,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_segment_pkey DO NOTHING;";

                var query = @"
                    INSERT INTO route_network.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routesegment_kind,
                    routesegment_height,
                    routesegment_width,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeSegmentKind,
                    @routeSegmentHeight,
                    @routeSegmentWidth,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_segment_pkey DO NOTHING;";

                var mappedRouteSegment = new
                {
                    mrid = routeSegment.Mrid,
                    coord = routeSegment.Coord,
                    workTaskMrId = routeSegment.WorkTaskMrid,
                    userName = routeSegment.Username,
                    applicationName = routeSegment.ApplicationName,
                    applicationInfo = routeSegment.ApplicationInfo,
                    markAsDeleted = routeSegment.MarkAsDeleted,
                    deleteMe = routeSegment.DeleteMe,
                    lifeCycleDeploymentState = routeSegment.LifeCycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeSegment.LifeCycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeSegment.LifeCycleInfo?.RemovalDate,
                    mappingMethod = routeSegment.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeSegment.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeSegment.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeSegment.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeSegment.MappingInfo?.SurveyDate,
                    safetyClassification = routeSegment.SafetyInfo?.Classification,
                    safetyRemark = routeSegment.SafetyInfo?.Remark,
                    routeSegmentKind = routeSegment?.RouteSegmentInfo?.Kind?.ToString("g"),
                    routeSegmentHeight = routeSegment.RouteSegmentInfo?.Height,
                    routeSegmentWidth = routeSegment.RouteSegmentInfo?.Width,
                    namingName = routeSegment.NamingInfo?.Name,
                    namingDescription = routeSegment.NamingInfo?.Description
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(integratorQuery, mappedRouteSegment);
                await connection.ExecuteAsync(query, mappedRouteSegment);
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
                                @tolerance
                            ),
                            ST_GeomFromWKB(@coord, 25832)
                        )
                    )
                    FROM route_network_integrator.route_segment WHERE mrid = @mrid AND marked_to_be_deleted = false;
                ";

                await connection.OpenAsync();
                var result = await connection.QueryAsync<string>(query, new { routeNode.Coord, intersectingRouteSegment.Mrid, tolerance = _applicationSettings.Tolerance * 2 });

                return result.First();
            }
        }

        public async Task UpdateRouteSegment(RouteSegment routeSegment)
        {
            using (var connection = GetNpgsqlConnection())
            {
                var integratorQuery = @"
                    UPDATE route_network_integrator.route_segment
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                var query = @"
                    UPDATE route_network.route_segment
                    SET
                      coord = ST_GeomFromWKB(@coord, 25832),
                      work_task_mrid = @workTaskMrId,
                      user_name = @username,
                      application_name = @applicationName,
                      marked_to_be_deleted = @markAsDeleted
                    WHERE mrid = @mrid;";

                await connection.OpenAsync();
                await connection.ExecuteAsync(integratorQuery, routeSegment);
                await connection.ExecuteAsync(query, routeSegment);
            }
        }

        private bool AreAnyPropertiesNotNull<T>(object obj)
        {
            return typeof(T).GetProperties().Any(propertyInfo => propertyInfo.GetValue(obj) != null);
        }

        private NpgsqlConnection GetNpgsqlConnection()
        {
            return new NpgsqlConnection(
                        $"Host={_postgisSettings.Host};Port={_postgisSettings.Port};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}");
        }
    }
}
