using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Extension methods for <see cref="IndexRecommendation"/> that provide additional
/// functionality for working with index recommendations.
/// </summary>
public static class IndexRecommendationExtensions
{
    /// <summary>
    /// Determines whether this index recommendation includes the specified column.
    /// </summary>
    /// <param name="recommendation">The index recommendation to check.</param>
    /// <param name="columnName">The column name to search for (case-insensitive).</param>
    /// <returns>True if the column is either a key or included column; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> or <paramref name="columnName"/> is null.</exception>
    public static bool ContainsColumn(this IndexRecommendation recommendation, string columnName)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentException.ThrowIfNullOrEmpty(columnName);

        return recommendation.KeyColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase) ||
               recommendation.IncludeColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the total number of columns in this index recommendation (key + include columns).
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>The total column count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is null.</exception>
    public static int GetTotalColumnCount(this IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        return recommendation.KeyColumns.Count + recommendation.IncludeColumns.Count;
    }

    /// <summary>
    /// Gets a formatted string representation of the index recommendation suitable for display.
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>A formatted string showing key information about the recommendation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is null.</exception>
    public static string ToDisplayString(this IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return $"Index {recommendation.SuggestedName()} on {recommendation.Table} " +
               $"({FormatColumns(recommendation.KeyColumns)}) " +
               $"INCLUDE ({FormatColumns(recommendation.IncludeColumns)}) " +
               $"- Impact: {recommendation.EstimatedImpactPercent.ToString("F1", CultureInfo.InvariantCulture)}% " +
               $"Confidence: {recommendation.Confidence}";
    }

    /// <summary>
    /// Gets a brief summary of the index recommendation suitable for logging.
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>A concise summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is null.</exception>
    public static string ToSummaryString(this IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        var includeCols = recommendation.IncludeColumns.Count > 0
            ? $" INCLUDE ({string.Join(", ", recommendation.IncludeColumns)})"
            : string.Empty;

        return $"{recommendation.SuggestedName()} on {recommendation.Table} ({string.Join(", ", recommendation.KeyColumns)}){includeCols} " +
               $"- {recommendation.EstimatedImpactPercent.ToString("F1", CultureInfo.InvariantCulture)}% impact";
    }

    /// <summary>
    /// Gets the columns formatted as a comma-separated string.
    /// </summary>
    /// <param name="columns">The list of column names.</param>
    /// <returns>A formatted string of columns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="columns"/> is null.</exception>
    private static string FormatColumns(IReadOnlyList<string> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        return columns.Count > 0
            ? string.Join(", ", columns)
            : "(none)";
    }
}