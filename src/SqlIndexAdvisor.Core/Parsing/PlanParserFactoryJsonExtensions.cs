using System.Text.Json;
using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="PlanParserFactory"/>.
/// </summary>
public static class PlanParserFactoryJsonExtensions
{
	private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes the <see cref="PlanParserFactory"/> to a JSON string.
	/// </summary>
	/// <param name="value">The factory instance to serialize.</param>
	/// <param name="indented">Whether to indent the JSON for readability.</param>
	/// <returns>A JSON representation of the factory.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this PlanParserFactory value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true }
			: s_jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="PlanParserFactory"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized factory, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into a <see cref="PlanParserFactory"/> instance.</exception>
	public static PlanParserFactory? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<PlanParserFactory>(json, s_jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="PlanParserFactory"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized factory, or <see langword="null"/> on failure.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out PlanParserFactory? value)
	{
		ArgumentNullException.ThrowIfNull(json);

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
