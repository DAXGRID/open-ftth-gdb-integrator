using Microsoft.Extensions.Options;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.NotificationClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace OpenFTTH.GDBIntegrator.Producer.NotificationServer;

public class NotificationServerClient : INotificationClient
{
    private readonly Client _notificationClient;

    public NotificationServerClient(IOptions<NotificationServerSetting> setting)
    {
        var ipAddress = Dns.GetHostEntry(setting.Value.Domain).AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork);

        _notificationClient = new Client(
            ipAddress: ipAddress,
            port: setting.Value.Port,
            writeOnly: true);

        _notificationClient.Connect();
    }

    public void Notify(string notificationHeader, string notificationBody)
    {
        _notificationClient.Send(new(notificationHeader, notificationBody));
    }

    public void Dispose()
    {
        _notificationClient.Dispose();
    }
}
