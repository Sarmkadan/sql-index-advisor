using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// System.Text.Json helpers for <see cref="IndexRecommendation"/>: serialize to JSON and
/// deserialize back, using camelCase property names and string-valued enums.
/// </summary>
public static class IndexRecommendationJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes this index recommendation to a JSON string.
    /// </summary>
    /// <param name="value">The index recommendation to serialize.</param>
    /// <param name="indented">True to write indented JSON; false for compact output.</param>
    /// <returns>The JSON representation of the recommendation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this IndexRecommendation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Options are immutable once used, so indentation is applied through the writer instead.
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = indented }))
        {
            JsonSerializer.Serialize(writer, value, Options);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Deserializes an index recommendation from a JSON string.
    /// </summary>
    /// <param name="json">The JSON text to deserialize.</param>
    /// <returns>The deserialized recommendation, or null if the JSON is the literal <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON or does not match the recommendation shape.</exception>
    public static IndexRecommendation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<IndexRecommendation>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize an index recommendation from a JSON string without throwing on malformed input.
    /// </summary>
    /// <param name="json">The JSON text to deserialize.</param>
    /// <param name="value">When this method returns true, the deserialized recommendation; otherwise null.</param>
    /// <returns>True if a non-null recommendation was produced; otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out IndexRecommendation? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
