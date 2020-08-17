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

        public MappingMethodEnum? MapMappingMethod(string mappingMethodStringRepresentation)
        {
             if (string.IsNullOrEmpty(mappingMethodStringRepresentation))
                return null;

            switch (mappingMethodStringRepresentation)
            {
                case "DigitizedFromPaperMaps":
                    return MappingMethodEnum.DigitizedFromPaperMaps;
                case "Drafting":
                    return MappingMethodEnum.Drafting;
                case "Imagery":
                    return MappingMethodEnum.Imagery;
                case "LandSurveying":
                    return MappingMethodEnum.LandSurveying;
                case "Other":
                    return MappingMethodEnum.Other;
                case "Schematic":
                    return MappingMethodEnum.Schematic;
                case "Sensor":
                    return MappingMethodEnum.Sensor;
                default:
                    throw new ArgumentException($"Value '{mappingMethodStringRepresentation}' is not valid'");
            }
        }
    }
}
