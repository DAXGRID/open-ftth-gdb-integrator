using System;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper
{
    public class SerializationMapper : ISerializationMapper
    {
        public DeploymentStateEnum? MapDeploymentState(string deploymentStateStringRepresentation)
        {
            if (string.IsNullOrEmpty(deploymentStateStringRepresentation))
                return null;

            switch (deploymentStateStringRepresentation)
            {
                case "InService":
                    return DeploymentStateEnum.InService;
                case "Installed":
                    return DeploymentStateEnum.Installed;
                case "NotYetInstalled":
                    return DeploymentStateEnum.NotYetInstalled;
                case "OutOfService":
                    return DeploymentStateEnum.OutOfService;
                case "Removed":
                    return DeploymentStateEnum.Removed;
                default:
                    throw new ArgumentException($"Value '{deploymentStateStringRepresentation}' is not valid'");
            }
        }
    }
}
