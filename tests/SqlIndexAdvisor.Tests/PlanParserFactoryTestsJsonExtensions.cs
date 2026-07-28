using System;
using System.Text.Json;

/// <summary>
/// JSON serialization extensions for <see cref="PlanParserFactoryTests"/>.
/// </summary>
namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides System.Text.Json helpers for <see cref="PlanParserFactoryTests"/>.
    /// </summary>
    public static class PlanParserFactoryTestsJsonExtensions
    {
        // Cached serializer options with camel‑case naming.
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the specified <see cref="PlanParserFactoryTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output will be formatted with indentation.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this PlanParserFactoryTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var opts = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
            return JsonSerializer.Serialize(value, opts);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="PlanParserFactoryTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>
        /// The deserialized <see cref="PlanParserFactoryTests"/> instance, or <c>null</c> if <paramref name="json"/>
        /// is empty or consists only of whitespace.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to <see cref="PlanParserFactoryTests"/>.</exception>
        public static PlanParserFactoryTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<PlanParserFactoryTests>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="PlanParserFactoryTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">
        /// When this method returns, contains the deserialized <see cref="PlanParserFactoryTests"/> instance
        /// if the operation succeeded; otherwise <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out PlanParserFactoryTests? value)
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
}
