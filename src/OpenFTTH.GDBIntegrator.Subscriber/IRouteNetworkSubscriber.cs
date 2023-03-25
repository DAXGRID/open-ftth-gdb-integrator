using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Subscriber
{
    public interface IRouteNetworkSubscriber : IDisposable
    {
        Task Subscribe(
            int intervalMs,
            CancellationToken token = default);
    }
}
