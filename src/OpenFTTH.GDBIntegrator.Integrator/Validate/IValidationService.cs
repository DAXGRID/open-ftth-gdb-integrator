using System;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Validate
{
    public interface IValidationService
    {
        Task<bool> HasRelatedEquipment(Guid mrid);
    }
}
