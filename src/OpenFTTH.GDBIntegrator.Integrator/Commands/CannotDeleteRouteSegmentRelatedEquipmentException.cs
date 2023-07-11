using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands;

public class CannotDeleteRouteSegmentRelatedEquipmentException : Exception
{
    public CannotDeleteRouteSegmentRelatedEquipmentException()
    {
    }

    public CannotDeleteRouteSegmentRelatedEquipmentException(string message)
        : base(message)
    {
    }

    public CannotDeleteRouteSegmentRelatedEquipmentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
