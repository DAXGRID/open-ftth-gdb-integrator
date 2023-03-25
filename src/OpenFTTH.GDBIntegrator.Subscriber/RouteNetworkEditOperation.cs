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

    [JsonPropertyName("after")]
    public string After { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonConstructor]
    public RouteNetworkEditOperation(
        long sequenceNumber,
        Guid eventId,
        string before,
        string after,
        string type)
    {
        SequenceNumber = sequenceNumber;
        EventId = eventId;
        Before = before;
        After = after;
        Type = type;
    }
}
