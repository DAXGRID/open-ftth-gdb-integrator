using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFTTH.GDBIntegrator.Subscriber.Postgres;

public class GeoJsonConverter<T> : JsonConverter<T> where T : Geometry
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return new GeoJsonReader().Read<T>(doc.RootElement.GetRawText());
    }

    public override void Write(
        Utf8JsonWriter writer,
        T point,
        JsonSerializerOptions options)
    {
        // We will never serialize, so no reason to implement.
        throw new NotImplementedException();
    }
}
