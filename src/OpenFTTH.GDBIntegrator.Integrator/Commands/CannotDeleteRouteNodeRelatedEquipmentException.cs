using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands;

public class CannotDeleteRouteNodeRelatedEquipmentException : Exception
{
    public CannotDeleteRouteNodeRelatedEquipmentException()
    {
    }

    public CannotDeleteRouteNodeRelatedEquipmentException(string message)
        : base(message)
    {
    }

    public CannotDeleteRouteNodeRelatedEquipmentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
