using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Mapping
{
    public interface IInfoMapper
    {
        DeploymentStateEnum? MapDeploymentState(string deploymentStateStringRepresentation);
        MappingMethodEnum? MapMappingMethod(string mappingMethodStringRepresentation);
        RouteNodeKindEnum? MapRouteNodeKind(string routeNodeKindStringRepresentation);
        RouteNodeFunctionEnum? MapRouteNodeFunction(string routeNodeFunctionStringRepresentation);
        RouteSegmentKindEnum? MapRouteSegmentKind(string routeSegmentKindStringRepresentation);
    }
}
