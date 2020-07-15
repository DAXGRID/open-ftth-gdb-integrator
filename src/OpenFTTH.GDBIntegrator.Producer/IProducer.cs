using System;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Producer
{
    public interface IProducer : IDisposable
    {
        void Init();
        Task Produce(string topicName, Object message);
        Task Produce(string topicName, Object message, string partitionKey);
    }
}
