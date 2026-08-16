using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Holds constant values used by <see cref="RecommendationEngineJsonExtensions"/> to avoid magic literals.
/// </summary>
internal static class RecommendationEngineJsonExtensionsConstants
{
    /// <summary>
    /// The default <see cref="JsonSerializerDefaults"/> used when creating <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public const JsonSerializerDefaults SerializerDefaults = JsonSerializerDefaults.Web;

    /// <summary>
    /// The default property naming policy (camel‑case) for JSON serialization.
    /// </summary>
    public static readonly JsonNamingPolicy PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Indicates whether JSON output should be indented by default (false = compact).
    /// </summary>
    public const bool DefaultWriteIndented = false;

    /// <summary>
    /// The default condition for ignoring null values during serialization.
    /// </summary>
    public const JsonIgnoreCondition DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    /// <summary>
    /// The value used when the caller explicitly requests indented JSON.
    /// </summary>
    public const bool IndentedWriteIndented = true;
}
