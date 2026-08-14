using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    public static class ExecutionPlanJsonExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>A list of human-readable problems.</returns>
        public static IReadOnlyList<string> Validate(this ExecutionPlanJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No properties to validate in this class

            return problems.ToImmutableList();
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        public static bool IsValid(this ExecutionPlanJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the given <paramref name="value"/> is valid, throwing an exception if it's not.
        /// </summary>
        /// <param name="value">The value to ensure.</param>
        /// <exception cref="ArgumentException">If the value is not valid.</exception>
        public static void EnsureValid(this ExecutionPlanJsonExtensionsTests value)
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
