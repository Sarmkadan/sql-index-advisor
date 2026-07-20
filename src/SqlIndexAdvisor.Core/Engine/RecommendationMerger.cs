using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Merges duplicate index recommendations for the same table where one column set is a prefix of another.
/// Keeps the wider index (with more key columns) and merges include columns from both recommendations.
/// </summary>
public static class RecommendationMerger
{
    /// <summary>
    /// Merges a list of index recommendations, deduplicating recommendations for the same table.
    /// Two recommendations are considered duplicates if they target the same table with key columns
    /// where one is a prefix of the other. The wider index (with more key columns) is kept and
    /// absorbs the include columns and reasons from the narrower index.
    /// </summary>
    /// <param name="recommendations">The list of recommendations to merge.</param>
    /// <returns>A new list with merged recommendations.</returns>
    public static List<IndexRecommendation> Merge(List<IndexRecommendation> recommendations)
    {
        ArgumentNullException.ThrowIfNull(recommendations);

        var kept = new List<IndexRecommendation>();

        foreach (var candidate in recommendations)
        {
            var dupIndex = FindMatchingIndex(kept, candidate);

            if (dupIndex < 0)
            {
                kept.Add(candidate);
                continue;
            }

            var existing = kept[dupIndex];
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

            kept[dupIndex] = new IndexRecommendation
            {
                Table = winner.Table,
                KeyColumns = winner.KeyColumns,
                IncludeColumns = includes,
                EstimatedImpactPercent = Math.Max(winner.EstimatedImpactPercent, loser.EstimatedImpactPercent),
                Confidence = (Confidence)Math.Max((int)winner.Confidence, (int)loser.Confidence),
                Reasons = reasons
            };
        }

        return kept;
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