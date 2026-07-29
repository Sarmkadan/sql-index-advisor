using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="IndexRecommendationTests"/>.
/// </summary>
public static class IndexRecommendationTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <see cref="IndexRecommendationTests"/> instance to JSON.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this IndexRecommendationTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into an <see cref="IndexRecommendationTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="IndexRecommendationTests"/> instance, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static IndexRecommendationTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<IndexRecommendationTests>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize the specified JSON string into an <see cref="IndexRecommendationTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized instance if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out IndexRecommendationTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexRecommendationTests>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
