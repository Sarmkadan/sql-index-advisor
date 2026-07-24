using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Marker type for JSON serialization operations related to PlanParserFactoryExtensions.
/// </summary>
public sealed class PlanParserFactoryExtensionsMarker
{
}

/// <summary>
/// Provides System.Text.Json serialization extensions for working with PlanParserFactoryExtensions.
/// </summary>
public static class PlanParserFactoryExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a <see cref="PlanParserFactoryExtensionsMarker"/> reference to JSON.
    /// </summary>
    /// <param name="value">The <see cref="PlanParserFactoryExtensionsMarker"/> reference.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON representation of the reference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this PlanParserFactoryExtensionsMarker value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true }
            : s_jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts JSON to a <see cref="PlanParserFactoryExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <returns>The <see cref="PlanParserFactoryExtensionsMarker"/> reference, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static PlanParserFactoryExtensionsMarker? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<PlanParserFactoryExtensionsMarker>(json, s_jsonOptions);
    }

    /// <summary>
    /// Attempts to convert JSON to a <see cref="PlanParserFactoryExtensionsMarker"/> reference.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <param name="value">The converted <see cref="PlanParserFactoryExtensionsMarker"/>, or <see langword="null"/> if the conversion fails.</param>
    /// <returns><see langword="true"/> if the conversion succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    public static bool TryFromJson(string json, out PlanParserFactoryExtensionsMarker? value)
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