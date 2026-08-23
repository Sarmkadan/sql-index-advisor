using System;
using System.Text.Json;
using SqlIndexAdvisor.Tests;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlIndexAdvisor.Tests
{
    public static class PlanParserFactoryJsonExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the given <see cref="PlanParserFactoryJsonExtensionsTests"/> instance to JSON.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">When true, includes indentation for readability (default: false).</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this PlanParserFactoryJsonExtensionsTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
            return System.Text.Json.JsonSerializer.Serialize(value, value.GetType(), options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="PlanParserFactoryJsonExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized instance or null if input is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when JSON is malformed.</exception>
        public static PlanParserFactoryJsonExtensionsTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<PlanParserFactoryJsonExtensionsTests>(json, _options);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="PlanParserFactoryJsonExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
        public static bool TryFromJson(string json, out PlanParserFactoryJsonExtensionsTests? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = System.Text.Json.JsonSerializer.Deserialize<PlanParserFactoryJsonExtensionsTests>(json, _options);
                return value != null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
