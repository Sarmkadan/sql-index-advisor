using System;
using System.Collections.Generic;
using System.Text.Json;
using SqlIndexAdvisor.Core.Model;
using static System.Text.Json.JsonSerializer;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for <see cref="ExecutionPlanTests"/>.
    /// </summary>
    public static class ExecutionPlanTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
;

        /// <summary>
        /// Serializes the given <see cref="ExecutionPlanTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">When true, includes indentation for readability.</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ExecutionPlanTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
            return Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into an <see cref="ExecutionPlanTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized object or null if input is null.</returns>
        /// <exception cref="JsonException">Thrown on deserialization errors.</exception>
        public static ExecutionPlanTests? FromJson(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            return Deserialize<ExecutionPlanTests>(json, _options);
        }

        /// <summary>
        /// Tries to deserialize a JSON string into an <see cref="ExecutionPlanTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object on success.</param>
        /// <returns>True if deserialization succeeded, false otherwise.</returns>
        public static bool TryFromJson(string json, out ExecutionPlanTests? value)
        {
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
