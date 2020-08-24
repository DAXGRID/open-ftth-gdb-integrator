using System;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Mapping
{
    public class InfoMapper : IInfoMapper
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

        public RouteNodeKindEnum? MapRouteNodeKind(string routeNodeKindStringRepresentation)
        {
              if (string.IsNullOrEmpty(routeNodeKindStringRepresentation))
                return null;

            switch (routeNodeKindStringRepresentation)
            {
                case "BuildingAccessPoint":
                    return RouteNodeKindEnum.BuildingAccessPoint;
                case "CabinetBig":
                    return RouteNodeKindEnum.CabinetBig;
                case "CabinetSmall":
                    return RouteNodeKindEnum.CabinetSmall;
                case "CentralOfficeBig":
                    return RouteNodeKindEnum.CentralOfficeBig;
                case "CentralOfficeMedium":
                    return RouteNodeKindEnum.CentralOfficeMedium;
                case "CentralOfficeSmall":
                    return RouteNodeKindEnum.CentralOfficeSmall;
                case "ConduitClosure":
                    return RouteNodeKindEnum.ConduitClosure;
                case "ConduitEnd":
                    return RouteNodeKindEnum.ConduitEnd;
                case "ConduitSimpleJunction":
                    return RouteNodeKindEnum.ConduitSimpleJunction;
                case "HandHole":
                    return RouteNodeKindEnum.HandHole;
                case "ManHole":
                    return RouteNodeKindEnum.ManHole;
                case "MultiDwellingUnit":
                    return RouteNodeKindEnum.MultiDwellingUnit;
                case "SingleDwellingUnit":
                    return RouteNodeKindEnum.SingleDwellingUnit;
                case "SpliceClosure":
                    return RouteNodeKindEnum.SpliceClosure;
                default:
                    throw new ArgumentException($"Value '{routeNodeKindStringRepresentation}' is not valid'");
            }
        }

        public RouteNodeFunctionEnum? MapRouteNodeFunction(string routeNodeFunctionStringRepresentation)
        {
            if (string.IsNullOrEmpty(routeNodeFunctionStringRepresentation))
                return null;

            switch (routeNodeFunctionStringRepresentation)
            {
                case "AccessibleConduitClosure":
                    return RouteNodeFunctionEnum.AccessibleConduitClosure;
                case "CustomerPremisesPoint":
                    return RouteNodeFunctionEnum.CustomerPremisesPoint;
                case "FlexPoint":
                    return RouteNodeFunctionEnum.FlexPoint;
                case "NonAccessibleConduitClosure":
                    return RouteNodeFunctionEnum.NonAccessibleConduitClosure;
                case "PrimaryNode":
                    return RouteNodeFunctionEnum.PrimaryNode;
                case "SecondaryNode":
                    return RouteNodeFunctionEnum.SecondaryNode;
                case "SplicePoint":
                    return RouteNodeFunctionEnum.SplicePoint;
                default:
                    throw new ArgumentException($"Value '{routeNodeFunctionStringRepresentation}' is not valid'");
            }
        }

        public RouteSegmentKindEnum? MapRouteSegmentKind(string routeSegmentKindStringRepresentation)
        {
            if (string.IsNullOrEmpty(routeSegmentKindStringRepresentation))
                return null;

            switch (routeSegmentKindStringRepresentation)
            {
                case "Arial":
                    return RouteSegmentKindEnum.Arial;
                case "Drilling":
                    return RouteSegmentKindEnum.Drilling;
                case "Indoor":
                    return RouteSegmentKindEnum.Indoor;
                case "MicroTrenching":
                    return RouteSegmentKindEnum.MicroTrenching;
                case "RoadCrossoverDrilling":
                    return RouteSegmentKindEnum.RoadCrossoverDrilling;
                case "RoadCrossoverDuctBank":
                    return RouteSegmentKindEnum.RoadCrossoverDuctBank;
                case "Tunnel":
                    return RouteSegmentKindEnum.Tunnel;
                case "Underground":
                    return RouteSegmentKindEnum.Underground;
                default:
                    throw new ArgumentException($"Value '{routeSegmentKindStringRepresentation}' is not valid'");
            }
        }
    }
}
