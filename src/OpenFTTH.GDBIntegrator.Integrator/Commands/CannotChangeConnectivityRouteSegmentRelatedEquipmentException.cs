using System;

namespace OpenFTTH.GDBIntegrator.Integrator.Commands;

public class CannotChangeConnectivityRouteSegmentRelatedEquipmentException : Exception
{
    public CannotChangeConnectivityRouteSegmentRelatedEquipmentException()
    {
    }

    public CannotChangeConnectivityRouteSegmentRelatedEquipmentException(string message)
        : base(message)
    {
    }

    public CannotChangeConnectivityRouteSegmentRelatedEquipmentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
