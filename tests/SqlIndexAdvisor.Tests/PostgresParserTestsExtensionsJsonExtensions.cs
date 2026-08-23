using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="PostgresParserTests"/>.
/// </summary>
public static class PostgresParserTestsExtensionsJsonExtensions
{
    /// <summary>
    /// Serializes a <see cref="PostgresParserTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="PostgresParserTests"/> instance to serialize.</param>
    /// <param name="indented">If set to <c>true</c> the JSON is indented for readability.</param>
    /// <returns>A JSON string representing the <see cref="PostgresParserTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this PostgresParserTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? IndentedOptions : CompactOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PostgresParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="PostgresParserTests"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON for a <see cref="PostgresParserTests"/> instance.</exception>
    public static PostgresParserTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<PostgresParserTests>(json, CompactOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PostgresParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the <see cref="PostgresParserTests"/> instance if the conversion succeeded,
    /// or <see langword="null"/> if it failed. This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="json"/> was successfully converted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out PostgresParserTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<PostgresParserTests>(json, CompactOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>Shared compact serializer options (camelCase property names).</summary>
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Shared indented serializer options (camelCase property names).</summary>
    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
