using System;
using System.Collections.Generic;

namespace SqlIndexAdvisor.Tests
{
    public static class IndexRecommendationJsonExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the given IndexRecommendationJsonExtensionsTests instance.
        /// </summary>
        /// <param name="value">The IndexRecommendationJsonExtensionsTests instance to validate.</param>
        /// <returns>A list of human-readable problems found during validation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the given value is null.</exception>
        public static IReadOnlyList<string> Validate(this IndexRecommendationJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No properties to validate, as IndexRecommendationJsonExtensionsTests only contains methods.

            return problems;
        }

        /// <summary>
        /// Checks if the given IndexRecommendationJsonExtensionsTests instance is valid.
        /// </summary>
        /// <param name="value">The IndexRecommendationJsonExtensionsTests instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the given value is null.</exception>
        public static bool IsValid(this IndexRecommendationJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the given IndexRecommendationJsonExtensionsTests instance is valid.
        /// </summary>
        /// <param name="value">The IndexRecommendationJsonExtensionsTests instance to ensure.</param>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, listing the problems found during validation.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the given value is null.</exception>
        public static void EnsureValid(this IndexRecommendationJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);

            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
            }
        }
    }
}
