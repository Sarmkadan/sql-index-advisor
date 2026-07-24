using System.Text.Json;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="IndexRecommendation"/>.
/// </summary>
public static class IndexRecommendationValidationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts an <see cref="IndexRecommendation"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The recommendation to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the recommendation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this IndexRecommendation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses an <see cref="IndexRecommendation"/> instance from JSON text.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized recommendation instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static IndexRecommendation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexRecommendation>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse an <see cref="IndexRecommendation"/> instance from JSON text.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="value">Receives the deserialized recommendation instance if successful; otherwise, null.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out IndexRecommendation? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<IndexRecommendation>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}