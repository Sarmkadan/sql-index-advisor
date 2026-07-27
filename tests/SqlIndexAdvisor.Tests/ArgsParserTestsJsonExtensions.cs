using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization helpers for the <see cref="ArgsParserTests"/> test class.
/// </summary>
public static class ArgsParserTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <see cref="ArgsParserTests"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="ArgsParserTests"/> instance to serialize.</param>
    /// <param name="indented">If set to <c>true</c>, the output will be indented.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this ArgsParserTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Use the cached options for non-indented output; create a new options instance for indented output.
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into an <see cref="ArgsParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="ArgsParserTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ArgsParserTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ArgsParserTests>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize the specified JSON string into an <see cref="ArgsParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="ArgsParserTests"/> instance, if the conversion succeeded, or <c>null</c> if it failed.</param>
    /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out ArgsParserTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ArgsParserTests>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
