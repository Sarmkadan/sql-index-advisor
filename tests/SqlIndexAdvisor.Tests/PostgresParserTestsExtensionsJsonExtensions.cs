using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

namespace SqlIndexAdvisor.Tests
{
    public static class PostgresParserTestsExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOption = new JsonSerializerOptions { WriteIndented = true };

        /// <summary>
        /// Converts the <see cref="PostgresParserTestsExtensions"/> object to a JSON string.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representing the object.</returns>
        public string ToJson(PostgresParserTestsExtensions value, bool indented = false)
        {
            return indented ? JsonSerializer.Serialize(value, JsonOption) : JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Parses a JSON string to a <see cref="PostgresParserTestsExtensions"/> object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>A <see cref="PostgresParserTestsExtensions"/> object.</returns>
        /// <exception cref="JsonException">Failed to parse the JSON string.</exception>
        public static PostgresParserTestsExtensions FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new JsonException("Input string is null or empty.");
            }

            try
            {
                return JsonSerializer.Deserialize<PostgresParserTestsExtensions>(json, JsonOption);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to parse JSON string.", ex);
            }
        }

        /// <summary>
        /// Attempts to parse a JSON string to a <see cref="PostgresParserTestsExtensions"/> object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="value">The output object.</param>
        /// <returns>True if the parsing was successful; otherwise, false.</returns>
        /// <exception cref="JsonException">Failed to parse the JSON string.</exception>
        public static bool TryFromJson(string json, out PostgresParserTestsExtensions? value)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<PostgresParserTestsExtensions>(json, JsonOption);
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