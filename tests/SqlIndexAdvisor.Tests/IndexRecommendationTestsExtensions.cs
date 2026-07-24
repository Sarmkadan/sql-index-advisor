using System.Globalization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="IndexRecommendationTests"/> that provide additional functionality
/// for testing index recommendation scenarios.
/// </summary>
public static class IndexRecommendationTestsExtensions
{
    /// <summary>
    /// Creates a test index recommendation with the specified table name and columns.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="tableName">The table name (e.g., "dbo.Users").</param>
    /// <param name="keyColumns">The key columns for the index.</param>
    /// <param name="includeColumns">The include columns for the index.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keyColumns"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="includeColumns"/> is null.</exception>
    public static IndexRecommendation CreateTestRecommendation(
        this IndexRecommendationTests tests,
        string tableName,
        IReadOnlyList<string> keyColumns,
        IReadOnlyList<string> includeColumns)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(keyColumns);
        ArgumentNullException.ThrowIfNull(includeColumns);

        return new IndexRecommendation
        {
            Table = tableName,
            KeyColumns = keyColumns.ToList(),
            IncludeColumns = includeColumns.ToList(),
            EstimatedImpactPercent = 85.5,
            SourceNodeCost = 0.75,
            Confidence = Confidence.High,
            Reasons = new List<string> { "Test recommendation for validation" }
        };
    }

    /// <summary>
    /// Creates a test index recommendation with only key columns.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="keyColumns">The key columns as params array.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keyColumns"/> is null.</exception>
    public static IndexRecommendation CreateKeyOnlyRecommendation(
        this IndexRecommendationTests tests,
        string tableName,
        params string[] keyColumns)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(keyColumns);

        return new IndexRecommendation
        {
            Table = tableName,
            KeyColumns = keyColumns.ToList(),
            IncludeColumns = new List<string>(),
            EstimatedImpactPercent = 50.0,
            SourceNodeCost = 0.5,
            Confidence = Confidence.Medium,
            Reasons = new List<string> { "Key-only recommendation" }
        };
    }

    /// <summary>
    /// Creates a test index recommendation with only include columns.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="includeColumns">The include columns as params array.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="includeColumns"/> is null.</exception>
    public static IndexRecommendation CreateIncludeOnlyRecommendation(
        this IndexRecommendationTests tests,
        string tableName,
        params string[] includeColumns)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(includeColumns);

        return new IndexRecommendation
        {
            Table = tableName,
            KeyColumns = new List<string>(),
            IncludeColumns = includeColumns.ToList(),
            EstimatedImpactPercent = 65.2,
            SourceNodeCost = 0.6,
            Confidence = Confidence.Low,
            Reasons = new List<string> { "Include-only recommendation" }
        };
    }

    /// <summary>
    /// Gets the column count for the recommendation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="recommendation">The index recommendation.</param>
    /// <param name="includeKeyColumns">Whether to count key columns (true) or include columns (false).</param>
    /// <returns>The count of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static int GetColumnCount(
        this IndexRecommendationTests tests,
        IndexRecommendation recommendation,
        bool includeKeyColumns = true)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return includeKeyColumns
            ? recommendation.KeyColumns.Count
            : recommendation.IncludeColumns.Count;
    }

    /// <summary>
    /// Determines whether the recommendation has any columns defined.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>True if the recommendation has at least one column; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static bool HasColumns(
        this IndexRecommendationTests tests,
        IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return recommendation.KeyColumns.Count > 0 ||
               recommendation.IncludeColumns.Count > 0;
    }

    /// <summary>
    /// Gets the estimated impact as a percentage string formatted for display.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>A formatted string like "85.5%".</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static string GetImpactPercentageString(
        this IndexRecommendationTests tests,
        IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return $"{recommendation.EstimatedImpactPercent.ToString("F1", CultureInfo.InvariantCulture)}%";
    }

    /// <summary>
    /// Creates a collection of test recommendations for bulk testing scenarios.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="count">The number of recommendations to create.</param>
    /// <param name="baseTableName">The base table name (appended with index).</param>
    /// <returns>An enumerable of test recommendations.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="baseTableName"/> is null.</exception>
    public static IEnumerable<IndexRecommendation> CreateTestRecommendations(
        this IndexRecommendationTests tests,
        int count,
        string baseTableName = "TestTable")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(baseTableName);

        for (var i = 0; i < count; i++)
        {
            yield return new IndexRecommendation
            {
                Table = $"{baseTableName}_{i}",
                KeyColumns = new List<string> { $"Id{i}" },
                IncludeColumns = new List<string> { $"Name{i}", $"Value{i}" },
                EstimatedImpactPercent = 10.0 + i * 5.0,
                SourceNodeCost = 0.1 + i * 0.05,
                Confidence = (Confidence)(i % 3),
                Reasons = new List<string> { $"Test reason {i}" }
            };
        }
    }
}
