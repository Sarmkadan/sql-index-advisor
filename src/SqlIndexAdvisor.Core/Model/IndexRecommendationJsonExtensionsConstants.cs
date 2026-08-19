using System.Text.Json;

namespace SqlIndexAdvisor.Core.Model;

internal static class IndexRecommendationJsonExtensionsConstants
{
    /// <summary>
    /// The default <see cref="JsonSerializerDefaults"/> used for the serializer options.
    /// </summary>
    public const JsonSerializerDefaults SerializerDefaults = JsonSerializerDefaults.Web;

    /// <summary>
    /// The naming policy applied to JSON property names.
    /// </summary>
    public static readonly JsonNamingPolicy NamingPolicy = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Indicates whether the serializer should write indented JSON by default.
    /// </summary>
    public const bool WriteIndentedDefault = false;

    /// <summary>
    /// Indicates whether the serializer should write indented JSON when explicitly requested.
    /// </summary>
    public const bool WriteIndentedIndented = true;
}
