using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="FullScanWithFilterRuleTests"/>.
    /// </summary>
    public static class FullScanWithFilterRuleTestsValidation
    {
        /// <summary>
        /// Validates the state of the supplied <see cref="FullScanWithFilterRuleTests"/> instance.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> of human‑readable problem descriptions.
        /// The list is empty when the instance is considered valid.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this FullScanWithFilterRuleTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // The test class only contains methods; there are no data members to validate.
            // Therefore, no validation problems can be detected.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the supplied <see cref="FullScanWithFilterRuleTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this FullScanWithFilterRuleTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            // No validation rules exist, so the instance is always valid.
            return true;
        }

        /// <summary>
        /// Ensures that the supplied <see cref="FullScanWithFilterRuleTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more validation problems are detected. The exception message contains a
        /// semicolon‑separated list of the problems.
        /// </exception>
        public static void EnsureValid(this FullScanWithFilterRuleTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = value.Validate();
            if (problems.Count > 0)
                throw new ArgumentException(string.Join("; ", problems));
        }
    }
}
