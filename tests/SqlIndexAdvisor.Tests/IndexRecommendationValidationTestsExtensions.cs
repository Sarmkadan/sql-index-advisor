using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="IndexRecommendationValidationTests"/> class.
    /// </summary>
    public static class IndexRecommendationValidationTestsExtensions
    {
        /// <summary>
        /// Retrieves a list of test methods that validate the <see cref="IndexRecommendation"/> class.
        /// </summary>
        /// <param name="tests">The <see cref="IndexRecommendationValidationTests"/> instance.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetValidationTestMethods(this IndexRecommendationValidationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
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
            };
        }

        /// <summary>
        /// Retrieves a list of test methods that validate the <see cref="IndexRecommendation"/> class for specific error cases.
        /// </summary>
        /// <param name="tests">The <see cref="IndexRecommendationValidationTests"/> instance.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetErrorTestMethods(this IndexRecommendationValidationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetValidationTestMethods().Where(method => method.Contains("ReturnsError")).ToList();
        }

        /// <summary>
        /// Retrieves a list of test methods that validate the <see cref="IndexRecommendation"/> class for specific valid cases.
        /// </summary>
        /// <param name="tests">The <see cref="IndexRecommendationValidationTests"/> instance.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetValidTestMethods(this IndexRecommendationValidationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetValidationTestMethods().Where(method => method.Contains("ReturnsEmptyList") || method.Contains("ReturnsTrue")).ToList();
        }
    }
}
