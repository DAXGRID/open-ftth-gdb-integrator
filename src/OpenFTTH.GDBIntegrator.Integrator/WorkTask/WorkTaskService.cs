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
            ApplicationSetting applicationSetting,
            HttpClient httpClient)
        {
            _applicationSetting = applicationSetting;
            _httpClient = httpClient;
        }

        public async Task<WorkTaskResponse> GetUserWorkTask(string userName)
        {
            var response = await _httpClient.GetAsync($"{_applicationSetting.ApiGatewayHost}/api/worktask/userworktask/{userName}");
            if (response.IsSuccessStatusCode)
            {
                throw new ApplicationException("Failed to receive user work task.");
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WorkTaskResponse>(result);
            }
        }
    }
}
