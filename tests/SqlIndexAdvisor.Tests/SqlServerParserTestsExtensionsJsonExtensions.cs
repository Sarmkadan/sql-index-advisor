using System;
using System.Text.Json;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="SqlServerParserTests"/>.
/// </summary>
public static class SqlServerParserTestsExtensionsJsonExtensions
{
    /// <summary>
    /// Serializes a <see cref="SqlServerParserTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SqlServerParserTests"/> instance to serialize.</param>
    /// <param name="indented">If set to <c>true</c> the JSON is indented for readability.</param>
    /// <returns>A JSON string representing the <see cref="SqlServerParserTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SqlServerParserTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SqlServerParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SqlServerParserTests"/> instance, or <see langword="null"/> if the JSON is invalid or represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON for a <see cref="SqlServerParserTests"/> instance.</exception>
    public static SqlServerParserTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<SqlServerParserTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SqlServerParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the <see cref="SqlServerParserTests"/> instance if the conversion succeeded, or <see langword="null"/> if it failed.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="json"/> was successfully converted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out SqlServerParserTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<SqlServerParserTests>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}