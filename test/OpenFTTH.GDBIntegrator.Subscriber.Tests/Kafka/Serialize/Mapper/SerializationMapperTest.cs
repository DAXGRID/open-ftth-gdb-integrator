using Xunit;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize.Mapper;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using FluentAssertions;
using System;

namespace OpenFTTH.GDBIntegrator.Subscriber.Tests.Kafka.Serialize
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
            var serializationMapper = new SerializationMapper();
            var result = serializationMapper.MapDeploymentState(deploymentStateStringReprestation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapDeploymentState_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new SerializationMapper();
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
            var serializationMapper = new SerializationMapper();
            var result = serializationMapper.MapMappingMethod(mappingMethodStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapMappingMethod_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new SerializationMapper();
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
        [InlineData("ConduitEnd", RouteNodeKindEnum.ConduitEnd)]
        [InlineData("ConduitSimpleJunction", RouteNodeKindEnum.ConduitSimpleJunction)]
        [InlineData("HandHole", RouteNodeKindEnum.HandHole)]
        [InlineData("ManHole", RouteNodeKindEnum.ManHole)]
        [InlineData("MultiDwellingUnit", RouteNodeKindEnum.MultiDwellingUnit)]
        [InlineData("SingleDwellingUnit", RouteNodeKindEnum.SingleDwellingUnit)]
        [InlineData("SpliceClosure", RouteNodeKindEnum.SpliceClosure)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void MapRouteNodeKind_ShouldReturnMappedRouteNodeKind_OnBeingPassedValidStringRepresentation(string routeNodeKindStringRepresentation, RouteNodeKindEnum? expected)
        {
            var serializationMapper = new SerializationMapper();
            var result = serializationMapper.MapRouteNodeKind(routeNodeKindStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapRouteNodeKind_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new SerializationMapper();
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
            var serializationMapper = new SerializationMapper();
            var result = serializationMapper.MapRouteNodeFunction(routeNodeFunctionStringRepresentation);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MapRouteNodeFunction_ShouldThrowArgumentException_OnBeingPassedInvalidStringRepresentation()
        {
            var serializationMapper = new SerializationMapper();
            serializationMapper.Invoking(x => x.MapRouteNodeFunction("InvalidText")).Should().ThrowExactly<ArgumentException>();
        }
    }
}
