using System;

namespace OpenFTTH.GDBIntegrator.Producer;

public interface INotificationClient : IDisposable
{
    void Notify(string notificationHeader, string notificationBody);
}
