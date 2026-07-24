using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Marker type for JSON serialization operations related to RecommendationEngineJsonExtensions.
/// </summary>
public sealed class RecommendationEngineJsonExtensionsMarker
{
}

/// <summary>
/// Provides System.Text.Json serialization extensions for working with RecommendationEngineJsonExtensions.
/// </summary>
public static class RecommendationEngineJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a <see cref="RecommendationEngineJsonExtensionsMarker"/> reference to JSON.
    /// </summary>
    /// <param name="value">The <see cref="RecommendationEngineJsonExtensionsMarker"/> reference.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON representation of the reference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this RecommendationEngineJsonExtensionsMarker value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? s_jsonOptions
            : new JsonSerializerOptions(s_jsonOptions) { WriteIndented = false };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts JSON to a <see cref="RecommendationEngineJsonExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <returns>The <see cref="RecommendationEngineJsonExtensionsMarker"/> reference, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static RecommendationEngineJsonExtensionsMarker? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<RecommendationEngineJsonExtensionsMarker>(json, s_jsonOptions);
    }

    /// <summary>
    /// Tries to convert JSON to a <see cref="RecommendationEngineJsonExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <param name="value">The converted <see cref="RecommendationEngineJsonExtensionsMarker"/>, or <see langword="null"/> if the conversion fails.</param>
    /// <returns><see langword="true"/> if the conversion succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    public static bool TryFromJson(string json, out RecommendationEngineJsonExtensionsMarker? value)
    {
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