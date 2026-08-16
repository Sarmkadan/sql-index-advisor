using System.Text.Json;

namespace SqlIndexAdvisor.Core.Model;

internal static class ExecutionPlanJsonExtensionsConstants
{
    /// <summary>
    /// The default <see cref="JsonSerializerDefaults"/> used for JSON serialization.
    /// </summary>
    public const JsonSerializerDefaults SerializerDefaults = JsonSerializerDefaults.Web;

    /// <summary>
    /// The default naming policy (camelCase) for JSON property names.
    /// </summary>
    public static readonly JsonNamingPolicy NamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// The default value for <see cref="JsonSerializerOptions.WriteIndented"/> (non‑indented).
    /// </summary>
    public const bool DefaultWriteIndented = false;

    /// <summary>
    /// The value for <see cref="JsonSerializerOptions.WriteIndented"/> when indentation is requested.
    /// </summary>
    public const bool IndentedWriteIndented = true;
}
