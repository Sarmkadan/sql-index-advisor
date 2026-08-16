using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Holds constant values used by <see cref="PlanParserFactoryJsonExtensions"/> to avoid magic literals.
/// </summary>
internal static class PlanParserFactoryJsonExtensionsConstants
{
    /// <summary>
    /// The default <see cref="JsonSerializerDefaults"/> used for the serializer options.
    /// </summary>
    public const JsonSerializerDefaults JsonSerializerDefaults = JsonSerializerDefaults.Web;

    /// <summary>
    /// The default naming policy (camel case) for JSON property names.
    /// </summary>
    public static readonly JsonNamingPolicy DefaultNamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// The default value for <see cref="JsonSerializerOptions.WriteIndented"/> (non‑indented).
    /// </summary>
    public const bool DefaultWriteIndented = false;

    /// <summary>
    /// The default condition for ignoring null values during serialization.
    /// </summary>
    public static readonly JsonIgnoreCondition DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    /// <summary>
    /// The value for <see cref="JsonSerializerOptions.WriteIndented"/> when indentation is requested.
    /// </summary>
    public const bool IndentedWriteIndented = true;
}
