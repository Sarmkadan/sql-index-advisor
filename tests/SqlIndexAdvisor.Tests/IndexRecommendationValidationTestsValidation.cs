using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="IndexRecommendationValidationTests"/>.
    /// </summary>
    public static class IndexRecommendationValidationTestsValidation
    {
        private static readonly string[] ExpectedMethodNames =
        {
            nameof(IndexRecommendationValidationTests.Validate_WithValidRecommendation_ReturnsEmptyList),
            nameof(IndexRecommendationValidationTests.Validate_WithNullTable_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithEmptyTable_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithNullKeyColumns_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithEmptyKeyColumns_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithWhitespaceKeyColumns_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithNullIncludeColumns_DoesNotAddError),
            nameof(IndexRecommendationValidationTests.Validate_WithWhitespaceIncludeColumns_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithInvalidEstimatedImpactPercent_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithMaxEstimatedImpactPercent_ReturnsNoError),
            nameof(IndexRecommendationValidationTests.Validate_WithNullReasons_DoesNotAddError),
            nameof(IndexRecommendationValidationTests.Validate_WithWhitespaceReasons_ReturnsError),
            nameof(IndexRecommendationValidationTests.Validate_WithMultipleProblems_ReturnsAllErrors),
            nameof(IndexRecommendationValidationTests.IsValid_WithValidRecommendation_ReturnsTrue),
            nameof(IndexRecommendationValidationTests.IsValid_WithInvalidRecommendation_ReturnsFalse),
            nameof(IndexRecommendationValidationTests.IsValid_WithNullRecommendation_ThrowsArgumentNullException),
            nameof(IndexRecommendationValidationTests.EnsureValid_WithValidRecommendation_DoesNotThrow),
            nameof(IndexRecommendationValidationTests.EnsureValid_WithInvalidRecommendation_ThrowsArgumentException),
            nameof(IndexRecommendationValidationTests.EnsureValid_WithNullRecommendation_ThrowsArgumentNullException),
            nameof(IndexRecommendationValidationTests.Validate_WithNullRecommendation_ThrowsArgumentNullException)
        };

        /// <summary>
        /// Validates that the <see cref="IndexRecommendationValidationTests"/> instance contains all expected test methods.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <returns>A read‑only list of validation error messages; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this IndexRecommendationValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();
            var type = typeof(IndexRecommendationValidationTests);
            var publicInstanceMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                            .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                                            .Select(m => m.Name)
                                            .ToHashSet(StringComparer.Ordinal);

            foreach (var expected in ExpectedMethodNames)
            {
                if (!publicInstanceMethods.Contains(expected))
                {
                    errors.Add($"Missing expected test method: {expected}");
                }
            }

            return errors;
        }

        /// <summary>
        /// Determines whether the supplied <see cref="IndexRecommendationValidationTests"/> instance passes validation.
        /// </summary>
        /// <param name="value">The test class instance to check.</param>
        /// <returns><c>true</c> if no validation errors are found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this IndexRecommendationValidationTests value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the supplied <see cref="IndexRecommendationValidationTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation errors are present; the message contains the list of problems.</exception>
        public static void EnsureValid(this IndexRecommendationValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join("; ", problems), nameof(value));
            }
        }
    }
}
