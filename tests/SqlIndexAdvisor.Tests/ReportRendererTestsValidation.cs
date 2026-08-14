using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="ReportRendererTests"/>.
    /// </summary>
    public static class ReportRendererTestsValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The <see cref="ReportRendererTests"/> instance to validate.</param>
        /// <returns>A list of human-readable problems.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ReportRendererTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No validation needed for methods, as they are not properties or fields
            return problems;
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The <see cref="ReportRendererTests"/> instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ReportRendererTests value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures the given <paramref name="value"/> is valid, throwing an exception if it's not.
        /// </summary>
        /// <param name="value">The <see cref="ReportRendererTests"/> instance to ensure is valid.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this ReportRendererTests value)
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
