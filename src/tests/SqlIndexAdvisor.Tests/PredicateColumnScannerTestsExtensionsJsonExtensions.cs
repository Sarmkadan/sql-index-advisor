using System;
using System.Text.Json;
using SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON (de)serialization helpers for <see cref="PredicateColumnScannerTestsExtensions"/>.
/// </summary>
public static class PredicateColumnScannerTestsExtensionsJsonExtensions
{
    // Cached serializer options with camel‑case naming.
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the supplied <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(PredicateColumnScannerTestsExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var opts = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, opts);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PredicateColumnScannerTestsExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static PredicateColumnScannerTestsExtensions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<PredicateColumnScannerTestsExtensions>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="PredicateColumnScannerTestsExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized instance if the operation succeeded; otherwise <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out PredicateColumnScannerTestsExtensions? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<PredicateColumnScannerTestsExtensions>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
