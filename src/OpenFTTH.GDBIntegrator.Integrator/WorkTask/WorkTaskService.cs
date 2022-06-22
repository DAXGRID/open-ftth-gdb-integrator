using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenFTTH.GDBIntegrator.Config;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.WorkTask
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly ApplicationSetting _applicationSetting;
        private readonly HttpClient _httpClient;

        public WorkTaskService(
            IOptions<ApplicationSetting> applicationSetting,
            HttpClient httpClient)
        {
            _applicationSetting = applicationSetting.Value;
            _httpClient = httpClient;
        }

        public async Task<WorkTaskResponse> GetUserWorkTask(string userName)
        {
            var response = await _httpClient.GetAsync($"{_applicationSetting.ApiGatewayHost}/api/worktask/userworktask/{userName}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WorkTaskResponse>(result);
            }
            else
            {
                throw new ApplicationException("Failed to receive user work task.");
            }
        }
    }
}
