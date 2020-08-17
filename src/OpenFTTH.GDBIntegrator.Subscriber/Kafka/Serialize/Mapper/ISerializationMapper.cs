using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper
{
    public interface ISerializationMapper
    {
        DeploymentStateEnum? MapDeploymentState(string deploymentStateStringRepresentation);
    }
}
