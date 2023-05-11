using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase.Postgres.QueryModels;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.GeoDatabase.Postgres
{
    public class Postgis : IGeoDatabase
    {
        private readonly PostgisSetting _postgisSettings;
        private readonly ApplicationSetting _applicationSettings;
        private readonly IInfoMapper _infoMapper;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;

        public Postgis(IOptions<PostgisSetting> postgisSettings, IOptions<ApplicationSetting> applicationSettings, IInfoMapper infoMapper)
        {
            _postgisSettings = postgisSettings.Value;
            _applicationSettings = applicationSettings.Value;
            _infoMapper = infoMapper;
        }

        public async Task BeginTransaction()
        {
            var connection = GetNpgsqlConnection();
            _transaction = await connection.BeginTransactionAsync();
        }

        public async Task DisposeTransaction()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task DisposeConnection()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async Task Commit()
        {
            await _transaction.CommitAsync();
            await DisposeTransaction();
            await DisposeConnection();
        }

        public async Task RollbackTransaction()
        {
            await _transaction.RollbackAsync();
            await DisposeTransaction();
            await DisposeConnection();
        }

        public async Task<bool> RouteNodeInShadowTableExists(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT exists(SELECT 1 FROM route_network_integrator.route_node WHERE mrid = @mrid)";
            return await connection.QueryFirstAsync<bool>(query, new { mrid });
        }

        public async Task<RouteNode> GetRouteNodeShadowTable(Guid mrid, bool includeDeleted = false)
        {
            var connection = GetNpgsqlConnection();

            var query = @"SELECT
                    ST_AsBinary(coord) AS coord,
                    mrid,
                    work_task_mrid AS workTaskMrid,
                    user_name AS username,
                    application_name AS applicationName,
                    application_info AS applicationInfo,
                    marked_to_be_deleted AS markedToBeDeleted,
                    delete_me AS deleteMe,
                    lifecycle_deployment_state AS lifecycleDeploymentState,
                    lifecycle_installation_date AS lifecycleInstallationDate,
                    lifecycle_removal_date AS lifecycleRemovalDate,
                    mapping_method AS mappingMethod,
                    mapping_vertical_accuracy AS mappingVerticalAccuracy,
                    mapping_horizontal_accuracy AS mappingHorizontalAccuracy,
                    mapping_source_info AS mappingSourceInfo,
                    mapping_survey_date AS mappingSurveyDate,
                    safety_classification AS safetyClassification,
                    safety_remark AS safetyRemark,
                    routenode_kind AS routeNodeKind,
                    routenode_function AS routeNodeFunction,
                    naming_name AS namingName,
                    naming_description AS namingDescription
                    FROM route_network_integrator.route_node";

            if (!includeDeleted)
                query += " WHERE mrid = @mrid AND marked_to_be_deleted = false";
            else
                query += " WHERE mrid = @mrid";

            var routeNodes = (await connection.QueryAsync<RouteNodeQueryModel>(query, new { mrid }))
                .Select(x => new RouteNode
                {
                    Mrid = x.Mrid,
                    Coord = x.Coord,
                    WorkTaskMrid = x.WorkTaskMrid,
                    Username = x.Username,
                    ApplicationName = x.ApplicationName,
                    ApplicationInfo = x.ApplicationInfo,
                    MarkAsDeleted = x.MarkedToBeDeleted,
                    DeleteMe = x.DeleteMe,
                    LifeCycleInfo = new LifecycleInfo
                    {
                        DeploymentState = _infoMapper.MapDeploymentState(x.LifeCycleDeploymentState),
                        InstallationDate = x.LifeCycleInstallationDate,
                        RemovalDate = x.LifeCycleRemovalDate,
                    },
                    MappingInfo = new MappingInfo
                    {
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
                        Method = _infoMapper.MapMappingMethod(x.MappingMethod),
                        SourceInfo = x.MappingSourceInfo,
                        SurveyDate = x.MappingSurveyDate,
                        VerticalAccuracy = x.MappingVerticalAccuracy
                    },
                    NamingInfo = new NamingInfo
                    {
                        Description = x.NamingDescription,
                        Name = x.NamingName
                    },
                    RouteNodeInfo = new RouteNodeInfo
                    {
                        Function = _infoMapper.MapRouteNodeFunction(x.RouteNodeFunction),
                        Kind = _infoMapper.MapRouteNodeKind(x.RouteNodeKind)
                    },
                    SafetyInfo = new SafetyInfo
                    {
                        Classification = x.SafetyClassification,
                        Remark = x.SafetyRemark
                    }
                });

            return routeNodes.FirstOrDefault();
        }

        public async Task<bool> RouteSegmentInShadowTableExists(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT exists(SELECT 1 FROM route_network_integrator.route_segment WHERE mrid = @mrid)";
            return await connection.QueryFirstAsync<bool>(query, new { mrid });
        }

        public async Task<RouteSegment> GetRouteSegmentShadowTable(Guid mrid, bool includeDeleted = false)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT
                    ST_AsBinary(coord) AS coord,
                    mrid,
                    work_task_mrid AS workTaskMrid,
                    user_name AS username,
                    application_name AS applicationName,
                    application_info AS applicationInfo,
                    marked_to_be_deleted AS markedToBeDeleted,
                    delete_me AS deleteMe,
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
                    FROM route_network_integrator.route_segment";

            if (!includeDeleted)
                query += " WHERE mrid = @mrid AND marked_to_be_deleted = false";
            else
                query += " WHERE mrid = @mrid";

            var routeSegments = (await connection.QueryAsync<RouteSegmentQueryModel>(query, new { mrid }))
                .Select(x => new RouteSegment
                {
                    ApplicationInfo = x.ApplicationInfo,
                    ApplicationName = x.ApplicationName,
                    Coord = x.Coord,
                    Mrid = x.Mrid,
                    Username = x.Username,
                    WorkTaskMrid = x.WorkTaskMrid,
                    DeleteMe = x.DeleteMe,
                    MarkAsDeleted = x.MarkedToBedeleted,
                    MappingInfo = new MappingInfo
                    {
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
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

            return routeSegments.FirstOrDefault();
        }

        public async Task<List<RouteNode>> GetIntersectingStartRouteNodes(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT
                          ST_AsBinary(coord) AS coord,
                          mrid,
                          work_task_mrid AS workTaskMrid,
                          user_name AS username,
                          application_name AS applicationName,
                          application_info AS applicationInfo,
                          marked_to_be_deleted AS markedToBeDeleted,
                          delete_me AS deleteMe,
                          lifecycle_deployment_state AS lifecycleDeploymentState,
                          lifecycle_installation_date AS lifecycleInstallationDate,
                          lifecycle_removal_date AS lifecycleRemovalDate,
                          mapping_method AS mappingMethod,
                          mapping_vertical_accuracy AS mappingVerticalAccuracy,
                          mapping_horizontal_accuracy AS mappingHorizontalAccuracy,
                          mapping_source_info AS mappingSourceInfo,
                          mapping_survey_date AS mappingSurveyDate,
                          safety_classification AS safetyClassification,
                          safety_remark AS safetyRemark,
                          routenode_kind AS routeNodeKind,
                          routenode_function AS routeNodeFunction,
                          naming_name AS namingName,
                          naming_description AS namingDescription
                          FROM route_network_integrator.route_node
                          WHERE ST_Intersects(
                          ST_Buffer(
                              ST_StartPoint(
                              (SELECT coord FROM route_network_integrator.route_segment
                               WHERE mrid = @mrid)
                              ),
                              @tolerance
                          ), coord) AND marked_to_be_deleted = false";

            var routeNodes = (await connection.QueryAsync<RouteNodeQueryModel>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance }))
                .Select(x => new RouteNode
                {
                    Mrid = x.Mrid,
                    Coord = x.Coord,
                    WorkTaskMrid = x.WorkTaskMrid,
                    Username = x.Username,
                    ApplicationName = x.ApplicationName,
                    ApplicationInfo = x.ApplicationInfo,
                    MarkAsDeleted = x.MarkedToBeDeleted,
                    DeleteMe = x.DeleteMe,
                    LifeCycleInfo = new LifecycleInfo
                    {
                        DeploymentState = _infoMapper.MapDeploymentState(x.LifeCycleDeploymentState),
                        InstallationDate = x.LifeCycleInstallationDate,
                        RemovalDate = x.LifeCycleRemovalDate,
                    },
                    MappingInfo = new MappingInfo
                    {
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
                        Method = _infoMapper.MapMappingMethod(x.MappingMethod),
                        SourceInfo = x.MappingSourceInfo,
                        SurveyDate = x.MappingSurveyDate,
                        VerticalAccuracy = x.MappingVerticalAccuracy
                    },
                    NamingInfo = new NamingInfo
                    {
                        Description = x.NamingDescription,
                        Name = x.NamingName
                    },
                    RouteNodeInfo = new RouteNodeInfo
                    {
                        Function = _infoMapper.MapRouteNodeFunction(x.RouteNodeFunction),
                        Kind = _infoMapper.MapRouteNodeKind(x.RouteNodeKind)
                    },
                    SafetyInfo = new SafetyInfo
                    {
                        Classification = x.SafetyClassification,
                        Remark = x.SafetyRemark
                    }
                });

            return routeNodes.ToList();
        }

        public async Task<List<RouteNode>> GetIntersectingStartRouteNodes(byte[] coord)
        {
            var connection = GetNpgsqlConnection();
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

            var routeNodes = await connection.QueryAsync<RouteNode>(query, new { coord, _applicationSettings.Tolerance });

            return routeNodes.AsList();
        }

        public async Task<List<RouteNode>> GetIntersectingEndRouteNodes(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT
                          ST_AsBinary(coord) AS coord,
                          mrid,
                          work_task_mrid AS workTaskMrid,
                          user_name AS username,
                          application_name AS applicationName,
                          application_info AS applicationInfo,
                          marked_to_be_deleted AS markedToBeDeleted,
                          delete_me AS deleteMe,
                          lifecycle_deployment_state AS lifecycleDeploymentState,
                          lifecycle_installation_date AS lifecycleInstallationDate,
                          lifecycle_removal_date AS lifecycleRemovalDate,
                          mapping_method AS mappingMethod,
                          mapping_vertical_accuracy AS mappingVerticalAccuracy,
                          mapping_horizontal_accuracy AS mappingHorizontalAccuracy,
                          mapping_source_info AS mappingSourceInfo,
                          mapping_survey_date AS mappingSurveyDate,
                          safety_classification AS safetyClassification,
                          safety_remark AS safetyRemark,
                          routenode_kind AS routeNodeKind,
                          routenode_function AS routeNodeFunction,
                          naming_name AS namingName,
                          naming_description AS namingDescription
                          FROM route_network_integrator.route_node
                          WHERE ST_Intersects(
                          ST_Buffer(
                              ST_EndPoint(
                              (SELECT coord FROM route_network_integrator.route_segment
                               WHERE mrid = @mrid)
                              ),
                              @tolerance
                          ), coord) AND marked_to_be_deleted = false";

            var routeNodes = (await connection.QueryAsync<RouteNodeQueryModel>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance }))
                .Select(x => new RouteNode
                {
                    Mrid = x.Mrid,
                    Coord = x.Coord,
                    WorkTaskMrid = x.WorkTaskMrid,
                    Username = x.Username,
                    ApplicationName = x.ApplicationName,
                    ApplicationInfo = x.ApplicationInfo,
                    MarkAsDeleted = x.MarkedToBeDeleted,
                    DeleteMe = x.DeleteMe,
                    LifeCycleInfo = new LifecycleInfo
                    {
                        DeploymentState = _infoMapper.MapDeploymentState(x.LifeCycleDeploymentState),
                        InstallationDate = x.LifeCycleInstallationDate,
                        RemovalDate = x.LifeCycleRemovalDate,
                    },
                    MappingInfo = new MappingInfo
                    {
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
                        Method = _infoMapper.MapMappingMethod(x.MappingMethod),
                        SourceInfo = x.MappingSourceInfo,
                        SurveyDate = x.MappingSurveyDate,
                        VerticalAccuracy = x.MappingVerticalAccuracy
                    },
                    NamingInfo = new NamingInfo
                    {
                        Description = x.NamingDescription,
                        Name = x.NamingName
                    },
                    RouteNodeInfo = new RouteNodeInfo
                    {
                        Function = _infoMapper.MapRouteNodeFunction(x.RouteNodeFunction),
                        Kind = _infoMapper.MapRouteNodeKind(x.RouteNodeKind)
                    },
                    SafetyInfo = new SafetyInfo
                    {
                        Classification = x.SafetyClassification,
                        Remark = x.SafetyRemark
                    }
                });

            return routeNodes.ToList();
        }

        public async Task<List<RouteNode>> GetIntersectingEndRouteNodes(byte[] coord)
        {
            var connection = GetNpgsqlConnection();
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

            var routeNodes = await connection.QueryAsync<RouteNode>(query, new { coord, _applicationSettings.Tolerance });

            return routeNodes.AsList();
        }

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
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
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
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

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(byte[] coordinates)
        {
            var connection = GetNpgsqlConnection();
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
                        ST_GeomFromWKB(@coordinates, 25832),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

            var routeSegments = (await connection.QueryAsync<RouteSegmentQueryModel>(query, new { coordinates, tolerance = _applicationSettings.Tolerance }))
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
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
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

        public async Task<List<RouteSegment>> GetIntersectingRouteSegments(RouteNode routeNode, RouteSegment notInclude)
        {
            var connection = GetNpgsqlConnection();
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
                        HorizontalAccuracy = x.MappingHorizontalAccuracy,
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

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodes(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
            var query = @"SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                          (SELECT coord FROM route_network_integrator.route_segment
                          WHERE mrid = @mrid),
                        @tolerance
                      ),
                      coord) AND marked_to_be_deleted = false";

            var result = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

            return result.AsList();
        }

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            var result = await connection.QueryAsync<RouteNode>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

            return result.AsList();
        }

        public async Task<List<RouteNode>> GetAllIntersectingRouteNodesNotIncludingEdges(byte[] coordinates, Guid mrid)
        {
            var connection = GetNpgsqlConnection();
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

            var result = await connection.QueryAsync<RouteNode>(query, new { mrid, coordinates, _applicationSettings.Tolerance });

            return result.AsList();
        }

        public async Task<List<RouteNode>> GetIntersectingRouteNodes(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
            var query = @"
                    SELECT ST_AsBinary(coord) AS coord, mrid
                    FROM route_network_integrator.route_node
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_GeomFromWKB(@coord, 25832),
                        @tolerance
                      ),
                      coord) AND mrid != @mrid AND marked_to_be_deleted = false
                    ";

            var routeNodes = await connection.QueryAsync<RouteNode>(query, new { routeNode.Mrid, routeNode.Coord, _applicationSettings.Tolerance });

            return routeNodes.AsList();
        }

        public async Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

            return routeSegments.AsList();
        }

        public async Task<List<RouteSegment>> GetIntersectingStartRouteSegments(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();

            var query = @"SELECT ST_AsBinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_GeomFromWKB(@coord, 25832),
                        @tolerance
                      ),
                      ST_StartPoint(coord)) AND marked_to_be_deleted = false
                    ";

            var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeNode.Coord, _applicationSettings.Tolerance });

            return routeSegments.AsList();
        }

        public async Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeSegment.Mrid, _applicationSettings.Tolerance });

            return routeSegments.AsList();
        }

        public async Task<List<RouteSegment>> GetIntersectingEndRouteSegments(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();

            var query = @"SELECT ST_Asbinary(coord) AS coord, mrid FROM route_network_integrator.route_segment
                    WHERE ST_Intersects(
                      ST_Buffer(
                        ST_GeomFromWKB(@coord, 25832),
                        @tolerance
                      ),
                      ST_EndPoint(coord)) AND marked_to_be_deleted = false
                    ";

            var routeSegments = await connection.QueryAsync<RouteSegment>(query, new { routeNode.Coord, _applicationSettings.Tolerance });

            return routeSegments.AsList();
        }

        public async Task DeleteRouteNode(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(intergratorQuery, new { mrid });
            await connection.ExecuteAsync(query, new { mrid });
        }

        public async Task DeleteRouteSegment(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(intergratorQuery, new { mrid });
            await connection.ExecuteAsync(query, new { mrid });
        }

        public async Task MarkDeleteRouteSegment(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(intergratorQuery, new { mrid });
            await connection.ExecuteAsync(query, new { mrid });
        }

        public async Task MarkDeleteRouteNode(Guid mrid)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(intergratorQuery, new { mrid });
            await connection.ExecuteAsync(query, new { mrid });
        }

        public async Task InsertRouteNode(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(intergratorQuery, routeNode);
            await connection.ExecuteAsync(query, routeNode);
        }

        public async Task UpdateRouteNode(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
            var integratorQuery = @"
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

            var query = @"
                    UPDATE route_network.route_node
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

            await connection.ExecuteAsync(integratorQuery, mappedRouteNode);
            await connection.ExecuteAsync(query, mappedRouteNode);
        }

        public async Task UpdateRouteNodeShadowTable(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(query, mappedRouteNode);
        }

        public async Task UpdateRouteNodeInfosShadowTable(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
            var query = @"
                    UPDATE route_network_integrator.route_node
                    SET
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

            await connection.ExecuteAsync(query, mappedRouteNode);
        }

        public async Task InsertRouteNodeShadowTable(RouteNode routeNode)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(query, mappedRouteNode);
        }

        public async Task InsertRouteSegmentShadowTable(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(query, mappedRouteSegment);
        }

        public async Task UpdateRouteSegmentShadowTable(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(query, mappedRouteSegment);
        }

        public async Task UpdateRouteSegmentInfosShadowTable(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
            var query = @"
                    UPDATE route_network_integrator.route_segment
                    SET
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

            await connection.ExecuteAsync(query, mappedRouteSegment);
        }

        public async Task InsertRouteSegment(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
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

            await connection.ExecuteAsync(integratorQuery, mappedRouteSegment);
            await connection.ExecuteAsync(query, mappedRouteSegment);
        }

        public async Task<string> GetRouteSegmentsSplittedByRouteNode(RouteNode routeNode, RouteSegment intersectingRouteSegment)
        {
            var connection = GetNpgsqlConnection();
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

            var result = await connection.QueryAsync<string>(query, new { routeNode.Coord, intersectingRouteSegment.Mrid, tolerance = _applicationSettings.Tolerance * 2 });

            return result.First();
        }

        public async Task UpdateRouteSegment(RouteSegment routeSegment)
        {
            var connection = GetNpgsqlConnection();
            var integratorQuery = @"
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

            var query = @"
                    UPDATE route_network.route_segment
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

            await connection.ExecuteAsync(integratorQuery, mappedRouteSegment);
            await connection.ExecuteAsync(query, mappedRouteSegment);
        }

        private bool AreAnyPropertiesNotNull<T>(object obj)
        {
            return typeof(T).GetProperties().Any(propertyInfo => propertyInfo.GetValue(obj) != null);
        }

        private NpgsqlConnection GetNpgsqlConnection()
        {
            if (_connection is null)
            {
                _connection = new NpgsqlConnection(
                        $"Host={_postgisSettings.Host};Port={_postgisSettings.Port};Username={_postgisSettings.Username};Password={_postgisSettings.Password};Database={_postgisSettings.Database}");

                _connection.Open();
            }

            return _connection;
        }
    }
}
