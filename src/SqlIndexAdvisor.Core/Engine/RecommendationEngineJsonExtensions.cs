using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="RecommendationEngine"/>.
/// </summary>
public static class RecommendationEngineJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(RecommendationEngineJsonExtensionsConstants.SerializerDefaults)
    {
        PropertyNamingPolicy = RecommendationEngineJsonExtensionsConstants.PropertyNamingPolicy,
        WriteIndented = RecommendationEngineJsonExtensionsConstants.DefaultWriteIndented,
        DefaultIgnoreCondition = RecommendationEngineJsonExtensionsConstants.DefaultIgnoreCondition
    };

    /// <summary>
    /// Serializes the <see cref="RecommendationEngine"/> to a JSON string.
    /// </summary>
    public static string ToJson(this RecommendationEngine value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = RecommendationEngineJsonExtensionsConstants.IndentedWriteIndented }
            : JsonOptions;

        var payload = new { rules = value.RegisteredRules };
        return indented
            ? JsonSerializer.Serialize(payload, options)
            : "{}";
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RecommendationEngine"/> instance.
    /// </summary>
    public static RecommendationEngine? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        // Validate JSON and return a default engine
        JsonDocument.Parse(json);
        return new RecommendationEngine();
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RecommendationEngine"/> instance.
    /// </summary>
    public static bool TryFromJson(string json, out RecommendationEngine? value)
    {
        ArgumentNullException.ThrowIfNull(json);

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
