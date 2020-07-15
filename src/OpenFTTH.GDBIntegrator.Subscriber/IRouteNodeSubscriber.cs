using System;

namespace OpenFTTH.GDBIntegrator.Subscriber
{
    public interface IRouteNodeSubscriber : IDisposable
    {
        void Subscribe();
    }
}
