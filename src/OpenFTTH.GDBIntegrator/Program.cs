using OpenFTTH.GDBIntegrator.Internal;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            using (var host = ContainerConfig.Configure())
            {
                await host.StartAsync();
                await host.WaitForShutdownAsync();
            }
        }
    }
}
