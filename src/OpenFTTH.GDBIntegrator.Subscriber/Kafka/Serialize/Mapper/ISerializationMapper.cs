using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper
{
    public interface ISerializationMapper
    {
        DeploymentStateEnum? MapDeploymentState(string deploymentStateStringRepresentation);
        MappingMethodEnum? MapMappingMethod(string mappingMethodStringRepresentation);
        RouteNodeKindEnum? MapRouteNodeKind(string routeNodeKindStringRepresentation);
        RouteNodeFunctionEnum? MapRouteNodeFunction(string routeNodeFunctionStringRepresentation);
    }
}
