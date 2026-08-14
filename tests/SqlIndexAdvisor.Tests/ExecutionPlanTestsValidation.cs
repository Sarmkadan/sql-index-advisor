using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SqlIndexAdvisor.Tests
{
    public static class ExecutionPlanTestsValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The <see cref="ExecutionPlanTests"/> instance to validate.</param>
        /// <returns>A list of human-readable problems.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ExecutionPlanTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Note: Since ExecutionPlanTests only contains test methods and no properties, 
            // there's nothing to validate in this case.
            return problems.ToImmutableList();
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The <see cref="ExecutionPlanTests"/> instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ExecutionPlanTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the given <paramref name="value"/> is valid. If not, throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="value">The <see cref="ExecutionPlanTests"/> instance to ensure.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
        public static void EnsureValid(this ExecutionPlanTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);

            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid ExecutionPlanTests: {string.Join(", ", problems)}", nameof(value));
            }
        }
    }
}
