using System;
using System.Threading.Tasks;
using Topos.Producer;

namespace OpenFTTH.GDBIntegrator.Producer
{
    public interface IProducer : IDisposable
    {
        void Init();
        Task Produce(string topicName, ToposMessage toposMessage);
        Task Produce(string topicName, ToposMessage toposMessage, string partitionKey);
    }
}
