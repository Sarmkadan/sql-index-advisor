using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods that provide JSON serialization helpers for <see cref="ReportRendererTests"/>.
/// </summary>
public static class ReportRendererTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> with camelCase property naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the current instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ReportRendererTests"/> instance to serialize.</param>
    /// <param name="indented">If set to <c>true</c>, the JSON is pretty-printed with indentation.</param>
    /// <returns>A JSON string representing the current instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this ReportRendererTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = _jsonOptions;
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ReportRendererTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ReportRendererTests"/> instance, or <c>null</c> if the JSON is empty or invalid.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c>, empty, or whitespace.</exception>
    public static ReportRendererTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<ReportRendererTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ReportRendererTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="ReportRendererTests"/> instance if the deserialization succeeded, or <c>null</c> if it failed.
    /// </param>
    /// <returns><c>true</c> if the deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c>, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out ReportRendererTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<ReportRendererTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}