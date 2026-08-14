using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="PlanParserFactoryExtensionsTests"/>.
    /// </summary>
    public static class PlanParserFactoryExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the state of the supplied <see cref="PlanParserFactoryExtensionsTests"/> instance
        /// and returns a read‑only list of human‑readable problem descriptions.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>A read‑only list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this PlanParserFactoryExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // The test class only contains methods; there is no mutable state to validate.
            // Returning an empty list indicates that the instance is considered valid.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the supplied <see cref="PlanParserFactoryExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this PlanParserFactoryExtensionsTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the supplied <see cref="PlanParserFactoryExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid; the message contains a semicolon‑separated list of problems.</exception>
        public static void EnsureValid(this PlanParserFactoryExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException($"PlanParserFactoryExtensionsTests validation failed: {string.Join("; ", problems)}", nameof(value));
            }
        }
    }
}
