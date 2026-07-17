using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="SqlIndexAdvisor.Core.Model.IndexRecommendation"/> objects.
/// </summary>
public static class RecommendationEngineTestsJsonExtensions
{
	/// <summary>
	/// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
	/// </summary>
	private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the specified <see cref="SqlIndexAdvisor.Core.Model.IndexRecommendation"/> instance to JSON.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether the output should be indented.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
	public static string ToJson(this SqlIndexAdvisor.Core.Model.IndexRecommendation value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		var options = new JsonSerializerOptions(_options) { WriteIndented = indented };
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes the specified JSON string into an <see cref="SqlIndexAdvisor.Core.Model.IndexRecommendation"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="SqlIndexAdvisor.Core.Model.IndexRecommendation"/> instance, or <c>null</c> if deserialization fails.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
	public static SqlIndexAdvisor.Core.Model.IndexRecommendation? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<SqlIndexAdvisor.Core.Model.IndexRecommendation>(json, _options);
	}

	/// <summary>
	/// Tries to deserialize the specified JSON string into an <see cref="SqlIndexAdvisor.Core.Model.IndexRecommendation"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">When this method returns, contains the deserialized instance if successful; otherwise, <c>null</c>.</param>
	/// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
	public static bool TryFromJson(string json, out SqlIndexAdvisor.Core.Model.IndexRecommendation? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<SqlIndexAdvisor.Core.Model.IndexRecommendation>(json, _options);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}