using System;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// JSON serialization helpers for <see cref="RecommendationEngineJsonExtensionsTests"/>.
    /// </summary>
    public static class RecommendationEngineJsonExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="RecommendationEngineJsonExtensionsTests"/> instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this RecommendationEngineJsonExtensionsTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var opts = indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options;
            return JsonSerializer.Serialize(value, opts);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="RecommendationEngineJsonExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>
        /// The deserialized <see cref="RecommendationEngineJsonExtensionsTests"/> instance,
        /// or <c>null</c> if <paramref name="json"/> is empty or whitespace.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        public static RecommendationEngineJsonExtensionsTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<RecommendationEngineJsonExtensionsTests>(json, Options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="RecommendationEngineJsonExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">
        /// When this method returns, contains the deserialized instance if the operation succeeded;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out RecommendationEngineJsonExtensionsTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            if (string.IsNullOrWhiteSpace(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<RecommendationEngineJsonExtensionsTests>(json, Options);
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
