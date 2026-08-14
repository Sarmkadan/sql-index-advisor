using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="IndexRecommendationExtensionsTests"/>.
    /// </summary>
    public static class IndexRecommendationExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the supplied <see cref="IndexRecommendationExtensionsTests"/> instance and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of problem descriptions. The list is empty when the instance is considered valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this IndexRecommendationExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // The test class only contains test methods; there are no stateful members to validate.
            // Returning an empty list indicates no validation problems.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the supplied <see cref="IndexRecommendationExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><c>true</c> if no validation problems were found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this IndexRecommendationExtensionsTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the supplied <see cref="IndexRecommendationExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation problems are found. The exception message contains a semicolon‑separated list of the problems.</exception>
        public static void EnsureValid(this IndexRecommendationExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                var message = string.Join("; ", problems);
                throw new ArgumentException(message, nameof(value));
            }
        }
    }
}
