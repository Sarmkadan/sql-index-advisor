using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON (de)serialization helpers for <see cref="PostgresParserTests"/>.
/// </summary>
public static class PostgresParserTestsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the specified <see cref="PostgresParserTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PostgresParserTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PostgresParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="PostgresParserTests"/> instance, or <c>null</c> if the JSON represents <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to <see cref="PostgresParserTests"/>.</exception>
    public static PostgresParserTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<PostgresParserTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PostgresParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="PostgresParserTests"/> instance
    /// if the operation succeeded; otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out PostgresParserTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<PostgresParserTests>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
