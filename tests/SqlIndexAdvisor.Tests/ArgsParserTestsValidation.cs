using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides validation helpers for <see cref="ArgsParserTests"/>.
/// </summary>
namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Extension methods that validate the state of an <see cref="ArgsParserTests"/> instance.
    /// </summary>
    public static class ArgsParserTestsValidation
    {
        /// <summary>
        /// Validates the supplied <see cref="ArgsParserTests"/> instance and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of problem descriptions. The list is empty when the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this ArgsParserTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // The test class only contains methods; there are no mutable state members to validate.
            // If future members (e.g., strings, numbers, dates) are added, validation logic can be extended here.

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the supplied <see cref="ArgsParserTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this ArgsParserTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the supplied <see cref="ArgsParserTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation problems are found; the exception message contains the list of problems.</exception>
        public static void EnsureValid(this ArgsParserTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ArgsParserTests instance is invalid: {string.Join("; ", problems)}",
                    nameof(value));
            }
        }
    }
}
