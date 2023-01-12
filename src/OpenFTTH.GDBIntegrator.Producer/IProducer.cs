using System;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Producer
{
    public interface IProducer
    {
        Task Produce(Guid streamId, object message);
    }
}
