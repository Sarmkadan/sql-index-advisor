using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests
{
    public static class ExecutionPlanValidationTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
;

        /// <summary>
        /// Serializes the specified validation tests to JSON.
        /// </summary>
        /// <param name="value">The validation tests instance to serialize.</param>
        /// <param name="indented">true to format the JSON with indentation; otherwise false.</param>
        /// <returns>JSON string representation of the validation tests.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this ExecutionPlanValidationTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into an ExecutionPlanValidationTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized ExecutionPlanValidationTests instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown if deserialization fails.</exception>
        public static ExecutionPlanValidationTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<ExecutionPlanValidationTests>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to deserialize ExecutionPlanValidationTests", ex);
            }
        }

        /// <summary>
        /// Tries to deserialize a JSON string into an ExecutionPlanValidationTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized instance if successful.</param>
        /// <returns>true if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, out ExecutionPlanValidationTests? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
