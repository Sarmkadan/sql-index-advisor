using System;
using System.Linq;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Extension methods for <see cref="IIndexRecommendation"/> that produce read-only, human-readable summaries.
/// </summary>
public static class IndexRecommendationSummaryExtensions
{
    /// <summary>
    /// Gets the reasons supporting this index recommendation joined into a single line.
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <param name="separator">The text placed between consecutive reasons.</param>
    /// <returns>The reasons joined by <paramref name="separator"/>, or a placeholder when there are none.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> or <paramref name="separator"/> is null.</exception>
    public static string GetReasonsSummary(this IIndexRecommendation recommendation, string separator = IndexRecommendationSummaryExtensionsConstants.DefaultReasonSeparator)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentNullException.ThrowIfNull(separator);

        return recommendation.Reasons.Any()
            ? string.Join(separator, recommendation.Reasons)
            : IndexRecommendationSummaryExtensionsConstants.NoReasonsPlaceholder;
    }
}
