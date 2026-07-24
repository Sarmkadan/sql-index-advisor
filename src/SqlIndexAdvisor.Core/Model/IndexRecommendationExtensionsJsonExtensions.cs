using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Marker type for JSON serialization operations related to IndexRecommendationExtensions.
/// </summary>
public sealed class IndexRecommendationExtensionsMarker
{
}

/// <summary>
/// Provides System.Text.Json serialization extensions for working with IndexRecommendationExtensions.
/// </summary>
public static class IndexRecommendationExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts an <see cref="IndexRecommendationExtensionsMarker"/> reference to JSON.
    /// </summary>
    /// <param name="value">The <see cref="IndexRecommendationExtensionsMarker"/> reference.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON representation of the reference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this IndexRecommendationExtensionsMarker value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts JSON to an <see cref="IndexRecommendationExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <returns>The <see cref="IndexRecommendationExtensionsMarker"/> reference, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static IndexRecommendationExtensionsMarker? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexRecommendationExtensionsMarker>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to convert JSON to an <see cref="IndexRecommendationExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <param name="value">The converted <see cref="IndexRecommendationExtensionsMarker"/>, or <see langword="null"/> if the conversion fails.</param>
    /// <returns><see langword="true"/> if the conversion succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    public static bool TryFromJson(string json, out IndexRecommendationExtensionsMarker? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

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