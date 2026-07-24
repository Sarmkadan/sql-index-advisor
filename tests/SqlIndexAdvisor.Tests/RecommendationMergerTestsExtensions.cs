using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="RecommendationMergerTests"/> and <see cref="RecommendationMergerConflictTests"/> classes.
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

        /// <summary>
        /// Runs all conflict resolution tests.
        /// </summary>
        /// <param name="tests">The instance of <see cref="RecommendationMergerConflictTests"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void RunAllConflictTests(this RecommendationMergerConflictTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Merge_WithOptimizerHintAndHeuristicRule_PrefersOptimizerHint();
            tests.Merge_WithHeuristicAndOptimizerHint_PrefersOptimizerHint();
            tests.Merge_TwoOptimizerHints_PicksHigherImpact();
            tests.Merge_TwoOptimizerHintsWithSameImpact_PicksOne();
            tests.Merge_HeuristicRulesWithoutOptimizerHint_MergesAsBefore();
            tests.Merge_WithSchemaFixImplicitConversionColumn_FiltersOutCreateIndex();
            tests.Merge_WithSchemaFixOnSameColumn_FiltersCreateIndexRecommendation();
            tests.Merge_WithMultipleSchemaFixes_FiltersAllAffectedCreateIndexRecommendations();
            tests.Merge_NullRecommendationsList_ThrowsArgumentNullException();
            tests.Merge_WithPrefixColumnsAndOptimizerHint_PrefersWiderIndexWithOptimizerHint();
            tests.Merge_WithPrefixColumnsBothOptimizerHints_PicksHigherImpact();
        }
    }
}
