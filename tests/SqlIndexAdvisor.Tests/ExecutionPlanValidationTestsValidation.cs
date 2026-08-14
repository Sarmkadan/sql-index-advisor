using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="ExecutionPlanValidationTests"/>.
    /// </summary>
    public static class ExecutionPlanValidationTestsValidation
    {
        /// <summary>
        /// Validates that the test class contains the expected public test methods.
        /// </summary>
        /// <param name="value">The test class instance.</param>
        /// <returns>
        /// A read‑only list of problem descriptions; empty if no problems were found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<string> Validate(this ExecutionPlanValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();
            var type = value.GetType();

            // Expected public test method names – taken from the repository description.
            var expected = new[]
            {
                "Validate_HappyPath_ForEachMajorPublicMethod_ReturnsNoProblems",
                "Validate_NullInput_ThrowsArgumentNullException",
                "IsValid_HappyPath_ForEachMajorPublicMethod_ReturnsTrue",
                "IsValid_NullInput_ThrowsArgumentNullException",
                "EnsureValid_HappyPath_ForEachMajorPublicMethod_DoesNotThrow",
                "EnsureValid_NullInput_ThrowsArgumentNullException",
                "EnsureValid_InvalidPlan_ThrowsArgumentException",
                "Validate_InvalidDialect_ReturnsProblem",
                "Validate_NaNEstimatedTotalCost_ReturnsProblem",
                "Validate_InfiniteEstimatedTotalCost_ReturnsProblem",
                "Validate_NullNodesCollection_ReturnsProblem",
                "Validate_NullNodeInCollection_ReturnsProblem",
                "Validate_EmptyOperator_ReturnsProblem",
                "Validate_NegativeEstimatedRows_ReturnsProblem",
                "Validate_NaNEstimatedRows_ReturnsProblem",
                "Validate_OutOfRangeRelativeCost_ReturnsProblem",
                "Validate_NullPredicateColumnsCollection_ReturnsProblem",
                "Validate_EmptyPredicateColumns_ReturnsNoProblem",
                "Validate_NullEngineMissingIndexesCollection_ReturnsProblem",
                "Validate_NullMissingIndex_ReturnsProblem"
            };

            foreach (var name in expected)
            {
                var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method is null)
                {
                    problems.Add($"Missing method '{name}'.");
                    continue;
                }

                // Ensure the method returns void and has no parameters.
                if (method.ReturnType != typeof(void))
                    problems.Add($"Method '{name}' must return void.");

                if (method.GetParameters().Length != 0)
                    problems.Add($"Method '{name}' must have no parameters.");
            }

            return problems;
        }

        /// <summary>
        /// Indicates whether the test class passes all validation checks.
        /// </summary>
        /// <param name="value">The test class instance.</param>
        /// <returns>
        /// <c>true</c> if no problems were found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public static bool IsValid(this ExecutionPlanValidationTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures the test class is valid, throwing an <see cref="ArgumentException"/>
        /// that lists all validation problems if any are found.
        /// </summary>
        /// <param name="value">The test class instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when validation problems are detected.
        /// </exception>
        public static void EnsureValid(this ExecutionPlanValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = value.Validate();
            if (problems.Count == 0) return;

            throw new ArgumentException(
                $"ExecutionPlanValidationTests is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}",
                nameof(value));
        }
    }
}
