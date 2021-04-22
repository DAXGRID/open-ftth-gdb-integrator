using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;

namespace OpenFTTH.GDBIntegrator.Integrator.Validate
{
    public class ValidationService : IValidationService
    {
        private readonly ApplicationSetting _applicationSetting;
        private readonly HttpClient _httpClient;

        public ValidationService(IOptions<ApplicationSetting> applicationSetting, HttpClient httpClient)
        {
            _applicationSetting = applicationSetting.Value;
            _httpClient = httpClient;
        }

        public async Task<bool> HasRelatedEquipment(Guid mrid)
        {
            var response = await _httpClient.GetAsync($"{_applicationSetting.ApiGatewayHost}/api/routenetwork/hasrelatedequipment/{mrid}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to receive has related equipment, received code: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();

            return Convert.ToBoolean(result);
        }
    }
}
