using System.Text.Json;
using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="PlanParserFactory"/>.
/// </summary>
public static class PlanParserFactoryJsonExtensions
{
	private static readonly JsonSerializerOptions s_jsonOptions = new(PlanParserFactoryJsonExtensionsConstants.JsonSerializerDefaults)
	{
		PropertyNamingPolicy = PlanParserFactoryJsonExtensionsConstants.DefaultNamingPolicy,
		WriteIndented = PlanParserFactoryJsonExtensionsConstants.DefaultWriteIndented,
		DefaultIgnoreCondition = PlanParserFactoryJsonExtensionsConstants.DefaultIgnoreCondition
	};

	/// <summary>
	/// Serializes the <see cref="PlanParserFactory"/> to a JSON string.
	/// </summary>
	public static string ToJson(this PlanParserFactory value, bool indented = PlanParserFactoryJsonExtensionsConstants.DefaultWriteIndented)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = PlanParserFactoryJsonExtensionsConstants.IndentedWriteIndented }
			: s_jsonOptions;

		var payload = new { registeredParsers = value.RegisteredParserNames };
		return JsonSerializer.Serialize(payload, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="PlanParserFactory"/> instance.
	/// </summary>
	public static PlanParserFactory? FromJson(string json)
	{
		if (json is null || string.IsNullOrWhiteSpace(json))
			throw new ArgumentException("JSON string cannot be null, empty, or whitespace.", nameof(json));

		// Validate it's proper JSON, then return a default factory
		JsonDocument.Parse(json);
		return new PlanParserFactory();
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="PlanParserFactory"/> instance.
	/// </summary>
	public static bool TryFromJson(string json, out PlanParserFactory? value)
	{
		if (json is null || string.IsNullOrWhiteSpace(json))
			throw new ArgumentException("JSON string cannot be null, empty, or whitespace.", nameof(json));

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
