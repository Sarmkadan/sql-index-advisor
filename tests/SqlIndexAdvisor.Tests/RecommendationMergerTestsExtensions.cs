using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="RecommendationMergerTests"/> class.
    /// </summary>
    public static class RecommendationMergerTestsExtensions
    {
        /// <summary>
        /// Runs all merge tests with the same columns.
        /// </summary>
        /// <param name="tests">The instance of <see cref="RecommendationMergerTests"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void RunAllSameColumnsTests(this RecommendationMergerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Merge_WithSameColumns_MergesCorrectly();
        }

        /// <summary>
        /// Runs all merge tests with prefix columns.
        /// </summary>
        /// <param name="tests">The instance of <see cref="RecommendationMergerTests"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void RunAllPrefixColumnsTests(this RecommendationMergerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Merge_WithPrefixColumns_MergesCorrectly();
        }

        /// <summary>
        /// Runs all merge tests with different tables.
        /// </summary>
        /// <param name="tests">The instance of <see cref="RecommendationMergerTests"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void RunAllDifferentTablesTests(this RecommendationMergerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Merge_WithDifferentTables_DoesNotMerge();
        }

        /// <summary>
        /// Runs all merge tests with non-prefix columns.
        /// </summary>
        /// <param name="tests">The instance of <see cref="RecommendationMergerTests"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void RunAllNonPrefixColumnsTests(this RecommendationMergerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Merge_WithNonPrefixColumns_DoesNotMerge();
        }
    }
}
