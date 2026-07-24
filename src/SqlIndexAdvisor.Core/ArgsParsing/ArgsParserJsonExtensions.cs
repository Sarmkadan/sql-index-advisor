using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Core.ArgsParsing;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ArgsParser.ParseResult"/>.
/// </summary>
public static class ArgsParserJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Converts a <see cref="ArgsParser.ParseResult"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ArgsParser.ParseResult"/> to convert.</param>
    /// <param name="indented">True to format the JSON with indentation; otherwise, false.</param>
    /// <returns>A JSON string representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this ArgsParser.ParseResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options);
    }

    /// <summary>
    /// Converts a JSON string to an <see cref="ArgsParser.ParseResult"/>.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>An <see cref="ArgsParser.ParseResult"/> represented by <paramref name="json"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static ArgsParser.ParseResult? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ArgsParser.ParseResult>(json, Options);
    }

    /// <summary>
    /// Tries to convert a JSON string to an <see cref="ArgsParser.ParseResult"/>.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">When this method returns, contains the <see cref="ArgsParser.ParseResult"/> represented by <paramref name="json"/>, if conversion succeeded; otherwise, null.</param>
    /// <returns>true if <paramref name="json"/> was converted successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ArgsParser.ParseResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ArgsParser.ParseResult>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}