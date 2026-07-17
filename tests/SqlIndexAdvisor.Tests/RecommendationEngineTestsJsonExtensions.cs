using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="RecommendationEngineTests"/>.
/// </summary>
public static class RecommendationEngineTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <see cref="RecommendationEngineTests"/> instance to JSON.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this RecommendationEngineTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = new JsonSerializerOptions(_options) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into a <see cref="RecommendationEngineTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="RecommendationEngineTests"/> instance, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static RecommendationEngineTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<RecommendationEngineTests>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize the specified JSON string into a <see cref="RecommendationEngineTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized instance if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out RecommendationEngineTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<RecommendationEngineTests>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
