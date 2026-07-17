using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides JSON serialization helpers for <see cref="PredicateColumnScannerTests"/>.
    /// </summary>
    public static class PredicateColumnScannerTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Converts the specified <see cref="PredicateColumnScannerTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="PredicateColumnScannerTests"/> instance to convert.</param>
        /// <param name="indented">Whether to format the JSON string with indentation.</param>
        /// <returns>A JSON string representation of the <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this PredicateColumnScannerTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="PredicateColumnScannerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="PredicateColumnScannerTests"/> instance if deserialization is successful; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static PredicateColumnScannerTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<PredicateColumnScannerTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="PredicateColumnScannerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="PredicateColumnScannerTests"/> instance if successful; otherwise, null.</param>
        /// <returns>True if deserialization is successful; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out PredicateColumnScannerTests? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = JsonSerializer.Deserialize<PredicateColumnScannerTests>(json, _jsonOptions);
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
