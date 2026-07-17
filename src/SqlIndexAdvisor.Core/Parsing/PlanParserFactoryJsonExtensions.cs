using System.Text.Json;
using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

public static class PlanParserFactoryJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts a <see cref="PlanParserFactory"/> to JSON.
    /// </summary>
    /// <param name="value">The <see cref="PlanParserFactory"/> to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON representation of the <see cref="PlanParserFactory"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PlanParserFactory value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? s_jsonOptions : new JsonSerializerOptions(s_jsonOptions)
        {
            WriteIndented = false,
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts JSON to a <see cref="PlanParserFactory"/>.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <returns>The <see cref="PlanParserFactory"/> represented by the JSON, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static PlanParserFactory? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<PlanParserFactory>(json, s_jsonOptions);
    }

    /// <summary>
    /// Tries to convert JSON to a <see cref="PlanParserFactory"/>.
    /// </summary>
    /// <param name="json">The JSON to convert.</param>
    /// <param name="value">The converted <see cref="PlanParserFactory"/>, or <c>null</c> if the conversion fails.</param>
    /// <returns><c>true</c> if the conversion succeeds; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or whitespace.</exception>
    public static bool TryFromJson(string json, out PlanParserFactory? value)
    {
        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
