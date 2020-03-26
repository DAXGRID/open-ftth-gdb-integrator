using OpenFTTH.GDBIntegrator.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace OpenFTTH.GDBIntegrator
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ContainerConfig.Configure();
            var startup = container.GetService<Startup>();
            startup.Start();
        }
    }
}
