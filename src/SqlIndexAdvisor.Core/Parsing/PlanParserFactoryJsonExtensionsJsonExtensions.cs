using System.Text.Json;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Marker type for JSON serialization operations related to PlanParserFactoryJsonExtensions.
/// </summary>
public sealed class PlanParserJsonExtensionsMarker
{
}

/// <summary>
/// Provides JSON serialization extensions for working with PlanParserFactoryJsonExtensions.
/// </summary>
public static class PlanParserFactoryJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts a <see cref="PlanParserJsonExtensionsMarker"/> reference to JSON.
    /// </summary>
    /// <param name="value">The <see cref="PlanParserJsonExtensionsMarker"/> reference.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON representation of the reference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PlanParserJsonExtensionsMarker value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? s_jsonOptions
            : new JsonSerializerOptions(s_jsonOptions) { WriteIndented = false };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts JSON to a <see cref="PlanParserJsonExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <returns>The <see cref="PlanParserJsonExtensionsMarker"/> reference, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static PlanParserJsonExtensionsMarker? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<PlanParserJsonExtensionsMarker>(json, s_jsonOptions);
    }

    /// <summary>
    /// Tries to convert JSON to a <see cref="PlanParserJsonExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <param name="value">The converted <see cref="PlanParserJsonExtensionsMarker"/>, or <c>null</c> if the conversion fails.</param>
    /// <returns><c>true</c> if the conversion succeeds; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or whitespace.</exception>
    public static bool TryFromJson(string json, out PlanParserJsonExtensionsMarker? value)
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