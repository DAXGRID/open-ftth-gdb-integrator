using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Integrator.Validate
{
    public class ValidationService : IValidationService
    {
        private readonly ApplicationSetting _applicationSetting;

        public ValidationService(IOptions<ApplicationSetting> applicationSetting)
        {
            _applicationSetting = applicationSetting.Value;
        }

        private class CanBeDeletedResponse
        {
            public RouteNetwork RouteNetwork { get; set; }
        }

        private class RouteNetwork
        {
            public RouteElement RouteElement { get; set; }
        }

        private class RouteElement
        {
            public bool HasRelatedEquipment { get; set; }
        }

        public async Task<bool> CanBeDeleted(Guid mrid)
        {
            Console.WriteLine(_applicationSetting.ApiGatewayHost);
            var graphQLClient = new GraphQLHttpClient($"{_applicationSetting.ApiGatewayHost}/graphql", new SystemTextJsonSerializer());

            var hasRelatedEquipmentRquest = new GraphQLRequest
            {
                Query = @"
                query HasRelatedEquipment($routeElementId: ID!) {
                  routeNetwork {
                    routeElement(id: $routeElementId) {
                      hasRelatedEquipment
                    }
                  }
                }",
                OperationName = "HasRelatedEquipment",
                Variables = new
                {
                    routeElementId = mrid
                }
            };

            var response = await graphQLClient.SendQueryAsync<CanBeDeletedResponse>(hasRelatedEquipmentRquest);
            return !response.Data.RouteNetwork.RouteElement.HasRelatedEquipment;
        }
    }
}
