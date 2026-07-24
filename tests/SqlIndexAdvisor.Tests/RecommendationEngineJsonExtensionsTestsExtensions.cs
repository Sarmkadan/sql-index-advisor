using System;
using System.Collections.Generic;
using System.Globalization;
using SqlIndexAdvisor.Core.Engine;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public static class RecommendationEngineJsonExtensionsTestsExtensions
    {
        /// <summary>
        /// Converts a collection of recommendation engines to their JSON representations.
        /// </summary>
        /// <param name="engines">The collection of engines to convert.</param>
        /// <param name="indented">Whether to produce indented JSON for better readability.</param>
        /// <returns>A read-only list of JSON strings representing each engine.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="engines"/> is null.</exception>
        public static IReadOnlyList<string> ToJson(this IEnumerable<RecommendationEngine> engines, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(engines);

            var result = new List<string>();
            foreach (var engine in engines)
            {
                result.Add(engine.ToJson(indented));
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Attempts to parse JSON strings into recommendation engines, returning a dictionary of results.
        /// </summary>
        /// <param name="jsonStrings">Collection of JSON strings to parse.</param>
        /// <param name="engines">Output dictionary mapping each JSON string to its parsed engine (or null if parsing failed).</param>
        /// <returns>True if parsing was attempted; false if <paramref name="jsonStrings"/> was null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonStrings"/> is null.</exception>
        public static bool TryFromJson(this IEnumerable<string> jsonStrings, out IReadOnlyDictionary<string, RecommendationEngine?> engines)
        {
            ArgumentNullException.ThrowIfNull(jsonStrings);

            var result = new Dictionary<string, RecommendationEngine?>();
            bool attempted = false;

            foreach (var json in jsonStrings)
            {
                attempted = true;
                if (RecommendationEngineJsonExtensions.TryFromJson(json, out var engine))
                {
                    result[json] = engine;
                }
                else
                {
                    result[json] = null;
                }
            }

            engines = result.AsReadOnly();
            return attempted;
        }

        /// <summary>
        /// Safely parses a collection of JSON strings into engines, filtering out null results.
        /// </summary>
        /// <param name="jsonStrings">Collection of JSON strings to parse.</param>
        /// <returns>Read-only list of successfully parsed engines.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonStrings"/> is null.</exception>
        public static IReadOnlyList<RecommendationEngine> FromJson(this IEnumerable<string> jsonStrings)
        {
            ArgumentNullException.ThrowIfNull(jsonStrings);

            var result = new List<RecommendationEngine>();
            foreach (var json in jsonStrings)
            {
                var engine = RecommendationEngineJsonExtensions.FromJson(json);
                if (engine is not null)
                {
                    result.Add(engine);
                }
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Validates that a collection of engines can be round-tripped through JSON serialization.
        /// </summary>
        /// <param name="engines">Collection of engines to validate.</param>
        /// <param name="indented">Whether to use indented JSON for the round-trip.</param>
        /// <returns>True if all engines round-tripped successfully; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="engines"/> is null.</exception>
        public static bool ValidateRoundTrip(this IEnumerable<RecommendationEngine> engines, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(engines);

            foreach (var engine in engines)
            {
                var json = engine.ToJson(indented);
                var parsed = RecommendationEngineJsonExtensions.FromJson(json);
                if (parsed is null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}