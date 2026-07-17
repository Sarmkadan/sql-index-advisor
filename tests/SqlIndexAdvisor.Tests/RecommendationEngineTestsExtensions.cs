using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="RecommendationEngineTests"/> that provide fluent assertions
/// and helper methods for testing index recommendation scenarios.
/// </summary>
public static class RecommendationEngineTestsExtensions
{
    /// <summary>
    /// Asserts that the recommendation contains the specified key columns.
    /// </summary>
    /// <param name="assertion">The assertion context.</param>
    /// <param name="expectedKeys">The expected key columns.</param>
    /// <exception cref="Xunit.Sdk.EqualException">Thrown when the key columns do not match.</exception>
    public static void HasKeyColumns(this Assert assertion, string[] expectedKeys, IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(expectedKeys);
        ArgumentNullException.ThrowIfNull(recommendation);

        Assert.Equal(expectedKeys, recommendation.KeyColumns);
    }

    /// <summary>
    /// Asserts that the recommendation contains the specified include columns.
    /// </summary>
    /// <param name="assertion">The assertion context.</param>
    /// <param name="expectedIncludes">The expected include columns.</param>
    /// <exception cref="Xunit.Sdk.EqualException">Thrown when the include columns do not match.</exception>
    public static void HasIncludeColumns(this Assert assertion, string[] expectedIncludes, IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(expectedIncludes);
        ArgumentNullException.ThrowIfNull(recommendation);

        Assert.Equal(expectedIncludes, recommendation.IncludeColumns);
    }

    /// <summary>
    /// Asserts that the recommendation has the specified confidence level.
    /// </summary>
    /// <param name="assertion">The assertion context.</param>
    /// <param name="expectedConfidence">The expected confidence level.</param>
    /// <exception cref="Xunit.Sdk.EqualException">Thrown when the confidence does not match.</exception>
    public static void HasConfidence(this Assert assertion, Confidence expectedConfidence, IndexRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        Assert.Equal(expectedConfidence, recommendation.Confidence);
    }

    /// <summary>
    /// Creates a minimal execution plan with a sequential scan for testing.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="dialect">The SQL dialect. Defaults to Postgres.</param>
    /// <returns>A new execution plan with a sequential scan node.</returns>
    public static ExecutionPlan CreateSeqScanPlan(this string tableName, PlanDialect dialect = PlanDialect.Postgres)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        return new ExecutionPlan
        {
            Dialect = dialect,
            EstimatedTotalCost = 100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = tableName,
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "id" }
                }
            }
        };
    }

    /// <summary>
    /// Creates a minimal execution plan with a clustered index scan for testing.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="dialect">The SQL dialect. Defaults to SqlServer.</param>
    /// <returns>A new execution plan with a clustered index scan node.</returns>
    public static ExecutionPlan CreateClusteredIndexScanPlan(
        this string tableName,
        PlanDialect dialect = PlanDialect.SqlServer)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        return new ExecutionPlan
        {
            Dialect = dialect,
            EstimatedTotalCost = 10,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Clustered Index Scan",
                    TableName = tableName,
                    EstimatedRows = 100,
                    EstimatedRowsRead = 10000,
                    RelativeCost = 0.8,
                    PredicateColumns = { "status" },
                    OutputColumns = { "id", "total", "customer_id" }
                }
            }
        };
    }

    /// <summary>
    /// Creates an index recommendation with the specified table and columns.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <param name="includeColumns">The include columns.</param>
    /// <returns>A new index recommendation.</returns>
    public static IndexRecommendation CreateIndexRecommendation(
        this string tableName,
        string[] keyColumns,
        string[]? includeColumns = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName);
        ArgumentNullException.ThrowIfNull(keyColumns);

        return new IndexRecommendation
        {
            Table = tableName,
            KeyColumns = keyColumns.ToList(),
            IncludeColumns = includeColumns?.ToList() ?? []
        };
    }

    /// <summary>
    /// Gets the generated index name from a CREATE INDEX statement.
    /// </summary>
    /// <param name="createStatement">The CREATE INDEX statement.</param>
    /// <returns>The index name (e.g., "IX_Orders_Status_CreatedAt").</returns>
    /// <exception cref="ArgumentException">Thrown when the statement is not a valid CREATE INDEX.</exception>
    public static string GetIndexName(this string createStatement)
    {
        ArgumentException.ThrowIfNullOrEmpty(createStatement);

        const string prefix = "CREATE INDEX ";
        if (!createStatement.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Statement must be a CREATE INDEX statement.", nameof(createStatement));
        }

        var indexOfOn = createStatement.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
        if (indexOfOn < 0)
        {
            throw new ArgumentException("Statement must contain ' ON ' clause.", nameof(createStatement));
        }

        return createStatement.Substring(prefix.Length, indexOfOn - prefix.Length).Trim();
    }

    /// <summary>
    /// Gets the table name from a CREATE INDEX statement.
    /// </summary>
    /// <param name="createStatement">The CREATE INDEX statement.</param>
    /// <returns>The table name.</returns>
    /// <exception cref="ArgumentException">Thrown when the statement is not a valid CREATE INDEX.</exception>
    public static string GetTableName(this string createStatement)
    {
        ArgumentException.ThrowIfNullOrEmpty(createStatement);

        const string onPrefix = " ON ";
        var indexOfOn = createStatement.IndexOf(onPrefix, StringComparison.OrdinalIgnoreCase);
        if (indexOfOn < 0)
        {
            throw new ArgumentException("Statement must contain ' ON ' clause.", nameof(createStatement));
        }

        var afterOn = createStatement.Substring(indexOfOn + onPrefix.Length).Trim();
        var indexOfSpace = afterOn.IndexOf(' ');
        return indexOfSpace < 0 ? afterOn : afterOn.Substring(0, indexOfSpace);
    }

    /// <summary>
    /// Gets the key columns from a CREATE INDEX statement.
    /// </summary>
    /// <param name="createStatement">The CREATE INDEX statement.</param>
    /// <returns>The key columns.</returns>
    /// <exception cref="ArgumentException">Thrown when the statement is not a valid CREATE INDEX.</exception>
    public static IReadOnlyList<string> GetKeyColumns(this string createStatement)
    {
        ArgumentException.ThrowIfNullOrEmpty(createStatement);

        var columnsStart = createStatement.IndexOf('(');
        var columnsEnd = createStatement.IndexOf(')', columnsStart);
        if (columnsStart < 0 || columnsEnd < 0)
        {
            throw new ArgumentException("Statement must contain column list in parentheses.", nameof(createStatement));
        }

        var columnsText = createStatement.Substring(columnsStart + 1, columnsEnd - columnsStart - 1);
        return columnsText.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets the include columns from a CREATE INDEX statement.
    /// </summary>
    /// <param name="createStatement">The CREATE INDEX statement.</param>
    /// <returns>The include columns, or empty list if none.</returns>
    /// <exception cref="ArgumentException">Thrown when the statement is not a valid CREATE INDEX.</exception>
    public static IReadOnlyList<string> GetIncludeColumns(this string createStatement)
    {
        ArgumentException.ThrowIfNullOrEmpty(createStatement);

        const string includePrefix = "INCLUDE (";
        var indexOfInclude = createStatement.IndexOf(includePrefix, StringComparison.OrdinalIgnoreCase);
        if (indexOfInclude < 0)
        {
            return Array.Empty<string>();
        }

        var columnsStart = indexOfInclude + includePrefix.Length;
        var columnsEnd = createStatement.IndexOf(')', columnsStart);
        if (columnsEnd < 0)
        {
            throw new ArgumentException("Statement must contain closing parenthesis for INCLUDE clause.", nameof(createStatement));
        }

        var columnsText = createStatement.Substring(columnsStart, columnsEnd - columnsStart);
        return columnsText.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
    }
}