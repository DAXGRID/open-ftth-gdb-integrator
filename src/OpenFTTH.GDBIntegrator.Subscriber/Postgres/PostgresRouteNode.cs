using NetTopologySuite.Geometries;
using System;
using System.Text.Json.Serialization;

namespace OpenFTTH.GDBIntegrator.Subscriber.Postgres;

// Specific implementation for representing the `RouteNode` table in the Postgres database.
public sealed record PostgresRouteNode
{
    [JsonPropertyName("mrid")]
    public Guid Mrid { get; init; }

    [JsonPropertyName("marked_to_be_deleted")]
    public bool MarkedToBeDeleted { get; init; }

    [JsonPropertyName("delete_me")]
    public bool DeleteMe { get; init; }

    [JsonPropertyName("work_task_mrid")]
    public Guid? WorkTaskMrid { get; init; }

    [JsonPropertyName("user_name")]
    public string UserName { get; init; }

    [JsonPropertyName("application_name")]
    public string ApplicationName { get; init; }

    [JsonPropertyName("application_info")]
    public string ApplicationInfo { get; init; }

    [JsonPropertyName("lifecycle_deployment_state")]
    public string LifecycleDeploymentState { get; init; }

    [JsonPropertyName("lifecycle_installation_date")]
    public DateTime? LifecycleInstallationDate { get; init; }

    [JsonPropertyName("lifecycle_removal_date")]
    public DateTime? LifecycleRemovalDate { get; init; }

    [JsonPropertyName("mapping_method")]
    public string MappingMethod { get; init; }

    [JsonPropertyName("mapping_vertical_accuracy")]
    public string MappingVerticalAccuracy { get; init; }

    [JsonPropertyName("mapping_horizontal_accuracy")]
    public string MappingHorizontalAccuracy { get; init; }

    [JsonPropertyName("mapping_source_info")]
    public string MappingSourceInfo { get; init; }

    [JsonPropertyName("mapping_survey_date")]
    public DateTime? MappingSurveyDate { get; init; }

    [JsonPropertyName("safety_classification")]
    public string SafetyClassification { get; init; }

    [JsonPropertyName("safety_remark")]
    public string SafetyRemark { get; init; }

    [JsonPropertyName("routenode_kind")]
    public string RouteNodeKind { get; init; }

    [JsonPropertyName("routenode_function")]
    public string RoutenodeFunction { get; init; }

    [JsonPropertyName("naming_name")]
    public string NamingName { get; init; }

    [JsonPropertyName("naming_description")]
    public string NamingDescription { get; init; }

    [JsonPropertyName("lifecycle_documentation_state")]
    public string LifecycleDocumentationState { get; init; }
}
