namespace OpenFTTH.GDBIntegrator.Config;

public record NotificationServerSetting
{
    public string Domain { get; init; }
    public int Port { get; init; }
}
