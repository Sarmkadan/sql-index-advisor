using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides System.Text.Json serialization and deserialization helpers for <see cref="PlanParserFactoryExtensionsTests"/>.
/// </summary>
public static class PlanParserFactoryExtensionsTestsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Converts the <see cref="PlanParserFactoryExtensionsTests"/> instance to its JSON string representation.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">If true, writes the JSON with indentation; otherwise, writes compact JSON.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this PlanParserFactoryExtensionsTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(Options) { WriteIndented = true }
            : Options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PlanParserFactoryExtensionsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static PlanParserFactoryExtensionsTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<PlanParserFactoryExtensionsTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="PlanParserFactoryExtensionsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized instance if successful; otherwise, null.</param>
    /// <returns>true if deserialization was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out PlanParserFactoryExtensionsTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
