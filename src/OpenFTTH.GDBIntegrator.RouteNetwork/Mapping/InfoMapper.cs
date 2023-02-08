using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Mapping
{
    public class InfoMapper : IInfoMapper
    {
        public DeploymentStateEnum? MapDeploymentState(string deploymentStateStringRepresentation)
        {
            if (string.IsNullOrEmpty(deploymentStateStringRepresentation))
                return null;

            switch (deploymentStateStringRepresentation.ToLower())
            {
                case "inservice":
                    return DeploymentStateEnum.InService;
                case "installed":
                    return DeploymentStateEnum.Installed;
                case "notyetinstalled":
                    return DeploymentStateEnum.NotYetInstalled;
                case "outofservice":
                    return DeploymentStateEnum.OutOfService;
                case "removed":
                    return DeploymentStateEnum.Removed;
                default:
                    throw new ArgumentException($"Value '{deploymentStateStringRepresentation}' is not valid'");
            }
        }

        public MappingMethodEnum? MapMappingMethod(string mappingMethodStringRepresentation)
        {
            if (string.IsNullOrEmpty(mappingMethodStringRepresentation))
                return null;

            switch (mappingMethodStringRepresentation.ToLower())
            {
                case "digitizedfrompapermaps":
                    return MappingMethodEnum.DigitizedFromPaperMaps;
                case "drafting":
                    return MappingMethodEnum.Drafting;
                case "imagery":
                    return MappingMethodEnum.Imagery;
                case "landsurveying":
                    return MappingMethodEnum.LandSurveying;
                case "other":
                    return MappingMethodEnum.Other;
                case "schematic":
                    return MappingMethodEnum.Schematic;
                case "sensor":
                    return MappingMethodEnum.Sensor;
                default:
                    throw new ArgumentException($"Value '{mappingMethodStringRepresentation}' is not valid'");
            }
        }

        public RouteNodeKindEnum? MapRouteNodeKind(string routeNodeKindStringRepresentation)
        {
            if (string.IsNullOrEmpty(routeNodeKindStringRepresentation))
                return null;

            switch (routeNodeKindStringRepresentation.ToLower())
            {
                case "buildingaccesspoint":
                    return RouteNodeKindEnum.BuildingAccessPoint;
                case "cabinetbig":
                    return RouteNodeKindEnum.CabinetBig;
                case "cabinetsmall":
                    return RouteNodeKindEnum.CabinetSmall;
                case "centralofficebig":
                    return RouteNodeKindEnum.CentralOfficeBig;
                case "centralofficemedium":
                    return RouteNodeKindEnum.CentralOfficeMedium;
                case "centralofficesmall":
                    return RouteNodeKindEnum.CentralOfficeSmall;
                case "conduitclosure":
                    return RouteNodeKindEnum.ConduitClosure;
                case "conduitclosurebranchoff":
                    return RouteNodeKindEnum.ConduitClosureBranchOff;
                case "conduitclosurefork":
                    return RouteNodeKindEnum.ConduitClosureFork;
                case "conduitclosurestraight":
                    return RouteNodeKindEnum.ConduitClosureStraight;
                case "conduitconnector":
                    return RouteNodeKindEnum.ConduitConnector;
                case "conduitconnectorbranchoff":
                    return RouteNodeKindEnum.ConduitConnectorBranchOff;
                case "conduitconnectorstraight":
                    return RouteNodeKindEnum.ConduitConnectorStraight;
                case "conduitend":
                    return RouteNodeKindEnum.ConduitEnd;
                case "handhole":
                    return RouteNodeKindEnum.HandHole;
                case "manhole":
                    return RouteNodeKindEnum.ManHole;
                case "multidwellingunit":
                    return RouteNodeKindEnum.MultiDwellingUnit;
                case "singledwellingunit":
                    return RouteNodeKindEnum.SingleDwellingUnit;
                case "spliceclosure":
                    return RouteNodeKindEnum.SpliceClosure;
                default:
                    throw new ArgumentException($"Value '{routeNodeKindStringRepresentation}' is not valid'");
            }
        }

        public RouteNodeFunctionEnum? MapRouteNodeFunction(string routeNodeFunctionStringRepresentation)
        {
            if (string.IsNullOrEmpty(routeNodeFunctionStringRepresentation))
                return null;

            switch (routeNodeFunctionStringRepresentation.ToLower())
            {
                case "accessibleconduitclosure":
                    return RouteNodeFunctionEnum.AccessibleConduitClosure;
                case "customerpremisespoint":
                    return RouteNodeFunctionEnum.CustomerPremisesPoint;
                case "flexpoint":
                    return RouteNodeFunctionEnum.FlexPoint;
                case "nonaccessibleconduitclosure":
                    return RouteNodeFunctionEnum.NonAccessibleConduitClosure;
                case "primarynode":
                    return RouteNodeFunctionEnum.PrimaryNode;
                case "secondarynode":
                    return RouteNodeFunctionEnum.SecondaryNode;
                case "splicepoint":
                    return RouteNodeFunctionEnum.SplicePoint;
                default:
                    throw new ArgumentException($"Value '{routeNodeFunctionStringRepresentation}' is not valid'");
            }
        }

        public RouteSegmentKindEnum? MapRouteSegmentKind(string routeSegmentKindStringRepresentation)
        {
            if (string.IsNullOrEmpty(routeSegmentKindStringRepresentation))
                return null;

            switch (routeSegmentKindStringRepresentation.ToLower())
            {
                case "arial":
                    return RouteSegmentKindEnum.Arial;
                case "drilling":
                    return RouteSegmentKindEnum.Drilling;
                case "indoor":
                    return RouteSegmentKindEnum.Indoor;
                case "microtrenching":
                    return RouteSegmentKindEnum.MicroTrenching;
                case "roadcrossoverdrilling":
                    return RouteSegmentKindEnum.RoadCrossoverDrilling;
                case "roadcrossoverductbank":
                    return RouteSegmentKindEnum.RoadCrossoverDuctBank;
                case "tunnel":
                    return RouteSegmentKindEnum.Tunnel;
                case "underground":
                    return RouteSegmentKindEnum.Underground;
                default:
                    throw new ArgumentException($"Value '{routeSegmentKindStringRepresentation}' is not valid'");
            }
        }
    }
}
