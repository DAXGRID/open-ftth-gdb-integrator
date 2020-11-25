using Xunit;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using FluentAssertions;
using System;

namespace OpenFTTH.GDBIntegrator.RouteNetwork.Tests.Mapping
{
    public class SerializationMapperTest
    {
        [Theory]
        [InlineData("InService", DeploymentStateEnum.InService)]
        [InlineData("Installed", DeploymentStateEnum.Installed)]
        [InlineData("NotYetInstalled", DeploymentStateEnum.NotYetInstalled)]
        [InlineData("OutOfService", DeploymentStateEnum.OutOfService)]
        [InlineData("Removed", DeploymentStateEnum.Removed)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapDeploymentState_ShouldReturnMappedEnumDeploymentState_OnBeingPassedValidStringRepresentation(string deploymentStateStringReprestation, DeploymentStateEnum? expected)
        {
            var serializationMapper = new InfoMapper();
            var result = serializationMapper.MapDeploymentState(deploymentStateStringReprestation);

            result.Should().BeEquivalentTo(expected);
        }

        public void MapDeploymentState_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new InfoMapper();
            serializationMapper.Invoking(x => x.MapDeploymentState("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData("DigitizedFromPaperMaps", MappingMethodEnum.DigitizedFromPaperMaps)]
        [InlineData("Drafting", MappingMethodEnum.Drafting)]
        [InlineData("Imagery", MappingMethodEnum.Imagery)]
        [InlineData("LandSurveying", MappingMethodEnum.LandSurveying)]
        [InlineData("Other", MappingMethodEnum.Other)]
        [InlineData("Schematic", MappingMethodEnum.Schematic)]
        [InlineData("Sensor", MappingMethodEnum.Sensor)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapMappingMethod_ShouldReturnMappedMappingMethod_OnBeingPassedValidStringRepresentation(string mappingMethodStringRepresentation, MappingMethodEnum? expected)
        {
            var serializationMapper = new InfoMapper();
            var result = serializationMapper.MapMappingMethod(mappingMethodStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapMappingMethod_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new InfoMapper();
            serializationMapper.Invoking(x => x.MapMappingMethod("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData("BuildingAccessPoint", RouteNodeKindEnum.BuildingAccessPoint)]
        [InlineData("CabinetBig", RouteNodeKindEnum.CabinetBig)]
        [InlineData("CabinetSmall", RouteNodeKindEnum.CabinetSmall)]
        [InlineData("CentralOfficeBig", RouteNodeKindEnum.CentralOfficeBig)]
        [InlineData("CentralOfficeMedium", RouteNodeKindEnum.CentralOfficeMedium)]
        [InlineData("CentralOfficeSmall", RouteNodeKindEnum.CentralOfficeSmall)]
        [InlineData("ConduitClosure", RouteNodeKindEnum.ConduitClosure)]
        [InlineData("ConduitClosureBranchOff", RouteNodeKindEnum.ConduitClosureBranchOff)]
        [InlineData("ConduitClosureFork", RouteNodeKindEnum.ConduitClosureFork)]
        [InlineData("ConduitClosureStraight", RouteNodeKindEnum.ConduitClosureStraight)]
        [InlineData("ConduitConnector", RouteNodeKindEnum.ConduitConnector)]
        [InlineData("ConduitConnectorBranchOff", RouteNodeKindEnum.ConduitConnectorBranchOff)]
        [InlineData("ConduitConnectorStraight", RouteNodeKindEnum.ConduitConnectorStraight)]
        [InlineData("ConduitEnd", RouteNodeKindEnum.ConduitEnd)]
        [InlineData("HandHole", RouteNodeKindEnum.HandHole)]
        [InlineData("ManHole", RouteNodeKindEnum.ManHole)]
        [InlineData("MultiDwellingUnit", RouteNodeKindEnum.MultiDwellingUnit)]
        [InlineData("SingleDwellingUnit", RouteNodeKindEnum.SingleDwellingUnit)]
        [InlineData("SpliceClosure", RouteNodeKindEnum.SpliceClosure)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapRouteNodeKind_ShouldReturnMappedRouteNodeKind_OnBeingPassedValidStringRepresentation(string routeNodeKindStringRepresentation, RouteNodeKindEnum? expected)
        {
            var serializationMapper = new InfoMapper();
            var result = serializationMapper.MapRouteNodeKind(routeNodeKindStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapRouteNodeKind_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new InfoMapper();
            serializationMapper.Invoking(x => x.MapRouteNodeKind("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData("AccessibleConduitClosure", RouteNodeFunctionEnum.AccessibleConduitClosure)]
        [InlineData("CustomerPremisesPoint", RouteNodeFunctionEnum.CustomerPremisesPoint)]
        [InlineData("FlexPoint", RouteNodeFunctionEnum.FlexPoint)]
        [InlineData("NonAccessibleConduitClosure", RouteNodeFunctionEnum.NonAccessibleConduitClosure)]
        [InlineData("PrimaryNode", RouteNodeFunctionEnum.PrimaryNode)]
        [InlineData("SecondaryNode", RouteNodeFunctionEnum.SecondaryNode)]
        [InlineData("SplicePoint", RouteNodeFunctionEnum.SplicePoint)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapRouteNodeFunction_ShouldReturnMappedRouteNodeFunction_OnBeingPassedValidStringRepresentation(string routeNodeFunctionStringRepresentation, RouteNodeFunctionEnum? expected)
        {
            var serializationMapper = new InfoMapper();
            var result = serializationMapper.MapRouteNodeFunction(routeNodeFunctionStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapRouteNodeFunction_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new InfoMapper();
            serializationMapper.Invoking(x => x.MapRouteNodeFunction("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData("Arial", RouteSegmentKindEnum.Arial)]
        [InlineData("Drilling", RouteSegmentKindEnum.Drilling)]
        [InlineData("Indoor", RouteSegmentKindEnum.Indoor)]
        [InlineData("MicroTrenching", RouteSegmentKindEnum.MicroTrenching)]
        [InlineData("RoadCrossoverDrilling", RouteSegmentKindEnum.RoadCrossoverDrilling)]
        [InlineData("RoadCrossoverDuctBank", RouteSegmentKindEnum.RoadCrossoverDuctBank)]
        [InlineData("Tunnel", RouteSegmentKindEnum.Tunnel)]
        [InlineData("Underground", RouteSegmentKindEnum.Underground)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapRouteSegmentKind_ShouldReturnMappedRouteSegmentKind_OnBeingPassedValidStringRepresentation(string routeSegmentKindStringRepresentation, RouteSegmentKindEnum? expected)
        {
            var serializationMapper = new InfoMapper();
            var result = serializationMapper.MapRouteSegmentKind(routeSegmentKindStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapRouteSegmentKind_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new InfoMapper();
            serializationMapper.Invoking(x => x.MapRouteNodeFunction("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }
    }
}
