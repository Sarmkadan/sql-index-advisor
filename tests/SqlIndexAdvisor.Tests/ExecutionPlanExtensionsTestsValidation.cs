using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="ExecutionPlanExtensionsTests"/>.
    /// </summary>
    public static class ExecutionPlanExtensionsTestsValidation
    {
        // Expected public instance test methods (name only). All should be parameter‑less and return void.
        private static readonly string[] ExpectedMethodNames =
        {
            nameof(ExecutionPlanExtensionsTests.GetScanCandidates_WithValidPlan_ReturnsScanNodes),
            nameof(ExecutionPlanExtensionsTests.GetScanCandidates_WithIndexOnlyScans_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetScanCandidates_WithEmptyPlan_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetScanCandidates_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetTotalScanCost_WithValidPlan_ReturnsSumOfScanCosts),
            nameof(ExecutionPlanExtensionsTests.GetTotalScanCost_WithEmptyPlan_ReturnsZero),
            nameof(ExecutionPlanExtensionsTests.GetTotalScanCost_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetScannedTables_WithValidPlan_ReturnsDistinctTableNames),
            nameof(ExecutionPlanExtensionsTests.GetScannedTables_WithEmptyPlan_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetScannedTables_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetPredicateColumns_WithValidPlan_ReturnsDistinctPredicateColumns),
            nameof(ExecutionPlanExtensionsTests.GetPredicateColumns_WithEmptyPlan_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetPredicateColumns_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetIncludeCandidateColumns_WithValidPlan_ReturnsNonPredicateOutputColumns),
            nameof(ExecutionPlanExtensionsTests.GetIncludeCandidateColumns_WithEmptyPlan_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetIncludeCandidateColumns_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetMissingIndexEqualityColumns_WithValidPlan_ReturnsEqualityColumns),
            nameof(ExecutionPlanExtensionsTests.GetMissingIndexEqualityColumns_WithEmptyPlan_ReturnsEmpty),
            nameof(ExecutionPlanExtensionsTests.GetMissingIndexEqualityColumns_WithNullPlan_ThrowsArgumentNullException),
            nameof(ExecutionPlanExtensionsTests.GetMissingIndexInequalityColumns_WithValidPlan_ReturnsInequalityColumns)
        };

        /// <summary>
        /// Validates that <paramref name="value"/> contains all expected test methods with the correct signatures.
        /// </summary>
        /// <param name="value">The instance of <see cref="ExecutionPlanExtensionsTests"/> to validate.</param>
        /// <returns>
        /// A read‑only list of problem descriptions. The list is empty when the instance is valid.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this ExecutionPlanExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();
            var type = typeof(ExecutionPlanExtensionsTests);
            var binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var methodName in ExpectedMethodNames)
            {
                var method = type.GetMethod(methodName, binding);
                if (method is null)
                {
                    problems.Add($"Missing method: {methodName}");
                    continue;
                }

                if (method.ReturnType != typeof(void))
                {
                    problems.Add($"Method '{methodName}' must return void, but returns {method.ReturnType.Name}");
                }

                if (method.GetParameters().Length != 0)
                {
                    problems.Add($"Method '{methodName}' must have no parameters, but has {method.GetParameters().Length}");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the supplied <paramref name="value"/> passes all validation checks.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this ExecutionPlanExtensionsTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the supplied <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more validation problems are detected. The exception message contains the list of problems.
        /// </exception>
        public static void EnsureValid(this ExecutionPlanExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count != 0)
            {
                throw new ArgumentException($"ExecutionPlanExtensionsTests validation failed: {string.Join("; ", problems)}", nameof(value));
            }
        }
    }
}
