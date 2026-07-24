using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Merges duplicate index recommendations for the same table where one column set is a prefix of another.
/// Keeps the wider index (with more key columns) and merges include columns from both recommendations.
/// When both an optimizer-native hint (EngineHintRule) and a heuristic rule fire for the same table,
/// the optimizer hint is preferred as it has real Impact % from the query optimizer.
/// Also suppresses index recommendations (Kind = CreateIndex) on columns that have implicit conversions
/// (SchemaFix recommendations) to prevent suggesting indexes that won't be used due to conversions.
/// </summary>
public static class RecommendationMerger
{
    /// <summary>
    /// Merges a list of index recommendations, deduplicating recommendations for the same table.
    /// Two recommendations are considered duplicates if they target the same table with key columns
    /// where one is a prefix of the other. The wider index (with more key columns) is kept and
    /// absorbs the include columns and reasons from the narrower index.
    /// When both an optimizer-native hint (EngineHintRule) and a heuristic rule fire for the same table,
    /// the optimizer hint is preferred as it has real Impact % from the query optimizer.
    /// Also suppresses index recommendations (Kind = CreateIndex) on columns that have implicit conversions
    /// (SchemaFix recommendations) to prevent suggesting indexes that won't be used due to conversions.
    /// </summary>
    /// <param name="recommendations">The list of recommendations to merge.</param>
    /// <returns>A new list with merged recommendations.</returns>
    public static List<IndexRecommendation> Merge(List<IndexRecommendation> recommendations)
    {
        ArgumentNullException.ThrowIfNull(recommendations);

        // First, collect all columns that have implicit conversions from SchemaFix recommendations
        var implicitConversionColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rec in recommendations)
        {
            if (rec.Kind == RecommendationKind.SchemaFix)
            {
                foreach (var column in rec.KeyColumns)
                {
                    implicitConversionColumns.Add(column);
                }
            }
        }

        // Filter out index recommendations that include columns with implicit conversions
        var filteredRecommendations = recommendations
            .Where(rec => rec.Kind == RecommendationKind.SchemaFix ||
                         !rec.KeyColumns.Any(col => implicitConversionColumns.Contains(col)))
            .ToList();

        var kept = new List<IndexRecommendation>();

        foreach (var candidate in filteredRecommendations)
        {
            var dupIndex = FindMatchingIndex(kept, candidate);

            if (dupIndex < 0)
            {
                kept.Add(candidate);
                continue;
            }

            var existing = kept[dupIndex];
            kept[dupIndex] = MergeRecommendations(existing, candidate);
        }

        return kept;
    }

    /// <summary>
    /// Merges two recommendations for the same table into a single recommendation.
    /// When both an optimizer-native hint (EngineHintRule) and a heuristic rule fire for the same table,
    /// the optimizer hint is preferred as it has real Impact % from the query optimizer.
    /// </summary>
    /// <param name="existing">The existing recommendation.</param>
    /// <param name="candidate">The new recommendation to merge.</param>
    /// <returns>A merged recommendation.</returns>
    private static IndexRecommendation MergeRecommendations(IndexRecommendation existing, IndexRecommendation candidate)
    {
        // Keep the one with more key columns (wider covers the narrower).
        var winner = candidate.KeyColumns.Count >= existing.KeyColumns.Count ? candidate : existing;
        var loser = ReferenceEquals(winner, candidate) ? existing : candidate;

        var includes = winner.IncludeColumns
            .Concat(loser.IncludeColumns)
            .Where(c => !winner.KeyColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var reasons = winner.Reasons
            .Concat(loser.Reasons)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Prefer optimizer-native hints (EngineHintRule) over heuristic rules
        // If either recommendation is from EngineHintRule, prefer it
        var isWinnerOptimizer = IsOptimizerNativeHint(winner);
        var isLoserOptimizer = IsOptimizerNativeHint(loser);

        if (isWinnerOptimizer && !isLoserOptimizer)
        {
            // Winner is optimizer, loser is heuristic - keep winner as-is
        }
        else if (!isWinnerOptimizer && isLoserOptimizer)
        {
            // Loser is optimizer, winner is heuristic - swap them
            (winner, loser) = (loser, winner);
        }
        else if (isWinnerOptimizer && isLoserOptimizer)
        {
            // Both are optimizer hints - keep the one with higher impact
            if (winner.EstimatedImpactPercent < loser.EstimatedImpactPercent)
            {
                (winner, loser) = (loser, winner);
            }
        }
        else
        {
            // Neither is optimizer-native, both are heuristic - merge as before
        }

        // Mark heuristic impact estimates distinctly in reasons when the winner is heuristic
        if (!IsOptimizerNativeHint(winner) && !isLoserOptimizer)
        {
            reasons.Add("Impact estimate is heuristic, not optimizer-reported.");
        }

        // Use the winner's impact (which may have been swapped to be the optimizer hint)
        var impact = winner.EstimatedImpactPercent;

        return new IndexRecommendation
        {
            Table = winner.Table,
            KeyColumns = winner.KeyColumns,
            IncludeColumns = includes,
            EstimatedImpactPercent = impact,
            Confidence = (Confidence)Math.Max((int)winner.Confidence, (int)loser.Confidence),
            Rule = winner.Rule ?? loser.Rule,
            Reasons = reasons
        };
    }

    /// <summary>
    /// Determines if a recommendation comes from an optimizer-native source (EngineHintRule).
    /// These recommendations have real impact percentages from the query optimizer itself.
    /// </summary>
    /// <param name="recommendation">The recommendation to check.</param>
    /// <returns>True if the recommendation is from an optimizer-native source; otherwise false.</returns>
    private static bool IsOptimizerNativeHint(IndexRecommendation recommendation)
    {
        return string.Equals(recommendation.Rule, "engine-hint", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Finds an existing recommendation that matches the candidate (either same or prefix relationship).
    /// </summary>
    /// <param name="existingRecommendations">List of already processed recommendations.</param>
    /// <param name="candidate">The recommendation to find a match for.</param>
    /// <returns>Index of matching recommendation, or -1 if no match found.</returns>
    private static int FindMatchingIndex(List<IndexRecommendation> existingRecommendations, IndexRecommendation candidate)
    {
        for (var i = 0; i < existingRecommendations.Count; i++)
        {
            var existing = existingRecommendations[i];
            if (IsSameOrPrefix(existing, candidate))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Determines if two index recommendations target the same table with key columns where one is a prefix of the other.
    /// </summary>
    /// <param name="a">First recommendation.</param>
    /// <param name="b">Second recommendation.</param>
    /// <returns>True if recommendations are the same or one is a prefix of the other; otherwise false.</returns>
    private static bool IsSameOrPrefix(IndexRecommendation a, IndexRecommendation b)
    {
        if (!string.Equals(a.Table, b.Table, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var min = Math.Min(a.KeyColumns.Count, b.KeyColumns.Count);
        if (min == 0)
        {
            return false;
        }

        // Check if all columns in the shorter list match the prefix of the longer list
        for (var i = 0; i < min; i++)
        {
            if (!string.Equals(a.KeyColumns[i], b.KeyColumns[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
