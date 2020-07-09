using System;

namespace OpenFTTH.GDBIntegrator.Subscriber
{
    public interface IRouteSegmentSubscriber : IDisposable
    {
        void Subscribe();
    }
}
