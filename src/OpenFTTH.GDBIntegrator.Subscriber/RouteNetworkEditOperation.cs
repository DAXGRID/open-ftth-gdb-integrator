using System;
using System.Text.Json.Serialization;

namespace OpenFTTH.GDBIntegrator.Subscriber;

public sealed record RouteNetworkEditOperation
{
    [JsonPropertyName("seq_no")]
    public long SequenceNumber { get; init; }

    [JsonPropertyName("event_id")]
    public Guid EventId { get; init; }

    [JsonPropertyName("before")]
    public string Before { get; init; }

    [JsonPropertyName("before_coord")]
    public byte[] BeforeCoord { get; init; }

    [JsonPropertyName("after")]
    public string After { get; init; }

    [JsonPropertyName("after_coord")]
    public byte[] AfterCoord { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonConstructor]
    public RouteNetworkEditOperation(
        long sequenceNumber,
        Guid eventId,
        string before,
        byte[] beforeCoord,
        string after,
        byte[] afterCoord,
        string type)
    {
        SequenceNumber = sequenceNumber;
        EventId = eventId;
        Before = before;
        BeforeCoord = beforeCoord;
        After = after;
        AfterCoord = afterCoord;
        Type = type;
    }
}
