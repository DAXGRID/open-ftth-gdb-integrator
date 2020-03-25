using System;

namespace OpenFTTH.GDBIntegrator.Subscriber
{
    public interface ISubscriber : IDisposable
    {
        void Subscribe();
    }
}
