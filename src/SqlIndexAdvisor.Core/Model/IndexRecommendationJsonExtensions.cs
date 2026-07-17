using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Model;

public static class IndexRecommendationJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Converts an <see cref="IndexRecommendation"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="IndexRecommendation"/> to convert.</param>
    /// <param name="indented">True to format the JSON with indentation; otherwise, false.</param>
    /// <returns>A JSON string representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this IndexRecommendation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options);
    }

    /// <summary>
    /// Converts a JSON string to an <see cref="IndexRecommendation"/>.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>An <see cref="IndexRecommendation"/> represented by <paramref name="json"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static IndexRecommendation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexRecommendation>(json, Options);
    }

    /// <summary>
    /// Tries to convert a JSON string to an <see cref="IndexRecommendation"/>.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">When this method returns, contains the <see cref="IndexRecommendation"/> represented by <paramref name="json"/>, if conversion succeeded; otherwise, null.</param>
    /// <returns>true if <paramref name="json"/> was converted successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static bool TryFromJson(string json, out IndexRecommendation? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexRecommendation>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
