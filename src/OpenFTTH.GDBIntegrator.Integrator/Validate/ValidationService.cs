using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace OpenFTTH.GDBIntegrator.Integrator.Validate
{
    public class ValidationService : IValidationService
    {
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
            var graphQLClient = new GraphQLHttpClient("http://api-gateway.openftth.local/graphql", new SystemTextJsonSerializer());

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
