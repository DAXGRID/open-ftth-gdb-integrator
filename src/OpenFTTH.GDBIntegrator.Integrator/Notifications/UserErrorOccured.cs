using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenFTTH.GDBIntegrator.Producer;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications;

public class UserErrorOccured : INotification
{
    public string ErrorCode { get; init; }
    public string Username { get; init; }

    public UserErrorOccured(
        string errorCode,
        string username)
    {
        ErrorCode = errorCode;
        Username = username;
    }
}

public class UserErrorOccurredHandler : INotificationHandler<UserErrorOccured>
{
    private readonly ILogger<UserErrorOccurredHandler> _logger;
    private readonly INotificationClient _notificationClient;

    public UserErrorOccurredHandler(
        ILogger<UserErrorOccurredHandler> logger,
        INotificationClient notificationClient)
    {
        _logger = logger;
        _notificationClient = notificationClient;
    }

    public Task Handle(UserErrorOccured request, CancellationToken token)
    {
        _logger.LogDebug($"Starting {nameof(UserErrorOccurredHandler)}.");

        _notificationClient.Notify(
            "UserErrorOccurred",
            JsonConvert.SerializeObject(request));

        return Task.CompletedTask;
    }
}
