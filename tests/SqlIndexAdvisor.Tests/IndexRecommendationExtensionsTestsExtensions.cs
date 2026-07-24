using System.Globalization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="IndexRecommendationExtensionsTests"/> that provide additional functionality
/// for testing index recommendation scenarios.
/// </summary>
public static class IndexRecommendationExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a test index recommendation with the specified table name and columns.
    /// </summary>
    /// <param name="tableName">The table name (e.g., "dbo.Users")</param>
    /// <param name="keyColumns">The key columns for the index.</param>
    /// <param name="includeColumns">The include columns for the index.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keyColumns"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="includeColumns"/> is null.</exception>
    public static IndexRecommendation CreateTestRecommendation(
        this IndexRecommendationExtensionsTests _,
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
            EstimatedImpactPercent = 75.5,
            Confidence = Confidence.Medium,
            Reasons = new List<string> { "Test recommendation" }
        };
    }

    /// <summary>
    /// Creates a test index recommendation with only key columns.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    public static IndexRecommendation CreateKeyOnlyRecommendation(
        this IndexRecommendationExtensionsTests _,
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
            Confidence = Confidence.Low,
            Reasons = new List<string> { "Key-only recommendation" }
        };
    }

    /// <summary>
    /// Creates a test index recommendation with only include columns.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="includeColumns">The include columns.</param>
    /// <returns>A new <see cref="IndexRecommendation"/> instance.</returns>
    public static IndexRecommendation CreateIncludeOnlyRecommendation(
        this IndexRecommendationExtensionsTests _,
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
            Confidence = Confidence.High,
            Reasons = new List<string> { "Include-only recommendation" }
        };
    }

    /// <summary>
    /// Gets the column names formatted as a comma-separated string.
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <param name="includeKeyColumns">Whether to include key columns (true) or include columns (false).</param>
    /// <returns>A formatted string of column names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static string GetColumnNamesString(
        this IndexRecommendationExtensionsTests _,
        IndexRecommendation recommendation,
        bool includeKeyColumns = true)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        var columns = includeKeyColumns
            ? recommendation.KeyColumns
            : recommendation.IncludeColumns;

        return string.Join(", ", columns);
    }

    /// <summary>
    /// Determines whether the recommendation has any columns (either key or include).
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>True if the recommendation has at least one column; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static bool HasAnyColumns(
        this IndexRecommendationExtensionsTests _,
        IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return recommendation.KeyColumns.Count > 0 ||
               recommendation.IncludeColumns.Count > 0;
    }

    /// <summary>
    /// Gets the confidence level as a display string.
    /// </summary>
    /// <param name="recommendation">The index recommendation.</param>
    /// <returns>The confidence level formatted as "High", "Medium", or "Low".</returns>
    /// <exception cref="ArgumentNullException"><paramref name="recommendation"/> is null.</exception>
    public static string GetConfidenceString(
        this IndexRecommendationExtensionsTests _,
        IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return recommendation.Confidence switch
        {
            Confidence.High => "High",
            Confidence.Medium => "Medium",
            Confidence.Low => "Low",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Creates a collection of test recommendations for bulk testing scenarios.
    /// </summary>
    /// <param name="count">The number of recommendations to create.</param>
    /// <param name="baseTableName">The base table name (appended with index).</param>
    /// <returns>An enumerable of test recommendations.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0.</exception>
    public static IEnumerable<IndexRecommendation> CreateTestRecommendations(
        this IndexRecommendationExtensionsTests _,
        int count,
        string baseTableName = "TestTable")
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
        }

        for (var i = 0; i < count; i++)
        {
            yield return new IndexRecommendation
            {
                Table = $"{baseTableName}_{i}",
                KeyColumns = new List<string> { $"Id{i}" },
                IncludeColumns = new List<string> { $"Name{i}", $"Value{i}" },
                EstimatedImpactPercent = 10.0 + i * 5.0,
                Confidence = (Confidence)(i % 3),
                Reasons = new List<string> { $"Test reason {i}" }
            };
        }
    }
}