using System;

namespace OpenFTTH.GDBIntegrator.Subscriber
{
    public interface IRouteNetworkSubscriber : IDisposable
    {
        void Subscribe();
    }
}
