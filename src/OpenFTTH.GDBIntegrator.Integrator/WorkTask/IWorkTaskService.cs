using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.WorkTask
{
    public interface IWorkTaskService
    {
        Task<WorkTaskResponse> GetUserWorkTask(string userName);
    }
}
