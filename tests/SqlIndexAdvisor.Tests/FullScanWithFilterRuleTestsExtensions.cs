using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="FullScanWithFilterRuleTests"/> that provide reusable test utilities
/// for testing <see cref="FullScanWithFilterRule"/> behavior with different execution plans.
/// </summary>
public static class FullScanWithFilterRuleTestsExtensions
{
    /// <summary>
    /// Creates a simple execution plan with a sequential scan node that has a filter predicate.
    /// </summary>
    /// <param name="tableName">Name of the table being scanned.</param>
    /// <returns>An execution plan with a Seq Scan node containing filter columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace.</exception>
    public static ExecutionPlan CreateSeqScanPlan(this string tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
        }

        return new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
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
    /// Creates an execution plan with a clustered index scan node that has a filter predicate.
    /// </summary>
    /// <param name="tableName">Name of the table being scanned.</param>
    /// <returns>An execution plan with a Clustered Index Scan node containing filter columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace.</exception>
    public static ExecutionPlan CreateClusteredIndexScanPlan(this string tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
        }

        return new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = tableName,
                    EstimatedRows = 5000,
                    EstimatedRowsRead = 5000000,
                    RelativeCost = 0.8,
                    PredicateColumns = { "status" },
                    OutputColumns = { "id", "total", "customer_id", "status" }
                }
            }
        };
    }

    /// <summary>
    /// Evaluates a rule against a plan and returns the recommendations as a read-only list.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="rule">The rule to evaluate.</param>
    /// <param name="plan">The execution plan to evaluate.</param>
    /// <returns>A read-only list of index recommendations.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tests"/> is null,
    /// <paramref name="rule"/> is null,
    /// or <paramref name="plan"/> is null.
    /// </exception>
    public static IReadOnlyList<IndexRecommendation> Evaluate(
        this FullScanWithFilterRuleTests tests,
        FullScanWithFilterRule rule,
        ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(plan);

        return rule.Evaluate(plan).ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates an execution plan with multiple scan nodes, useful for testing filtering behavior.
    /// </summary>
    /// <param name="nodes">Collection of plan nodes to include in the execution plan.</param>
    /// <returns>An execution plan with the specified nodes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="nodes"/> is null.</exception>
    public static ExecutionPlan CreateMultiNodePlan(this IEnumerable<PlanNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        return new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 100,
            Nodes = nodes.ToList()
        };
    }

    /// <summary>
    /// Creates a plan node with a table scan operator and filter predicate.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="predicateColumns">Columns used in the filter predicate.</param>
    /// <param name="relativeCost">Relative cost of this scan (0..1).</param>
    /// <returns>A plan node configured as a table scan with filter.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tableName"/> is null,
    /// or <paramref name="predicateColumns"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace.</exception>
    public static PlanNode CreateTableScanNode(
        this string tableName,
        IEnumerable<string> predicateColumns,
        double relativeCost)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(predicateColumns);

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
        }

        var node = new PlanNode
        {
            Operator = "Table Scan",
            TableName = tableName,
            EstimatedRows = 100,
            EstimatedRowsRead = 10000,
            RelativeCost = relativeCost
        };

        foreach (var col in predicateColumns)
        {
            node.PredicateColumns.Add(col);
        }

        return node;
    }

    /// <summary>
    /// Creates a plan node with an index scan operator and filter predicate.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="predicateColumns">Columns used in the filter predicate.</param>
    /// <param name="outputColumns">Columns returned by the scan (output list).</param>
    /// <param name="relativeCost">Relative cost of this scan (0..1).</param>
    /// <returns>A plan node configured as an index scan with filter.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tableName"/> is null,
    /// <paramref name="predicateColumns"/> is null,
    /// or <paramref name="outputColumns"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace.</exception>
    public static PlanNode CreateIndexScanNode(
        this string tableName,
        IEnumerable<string> predicateColumns,
        IEnumerable<string> outputColumns,
        double relativeCost)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(predicateColumns);
        ArgumentNullException.ThrowIfNull(outputColumns);

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
        }

        var node = new PlanNode
        {
            Operator = "Index Scan",
            TableName = tableName,
            EstimatedRows = 500,
            EstimatedRowsRead = 500000,
            RelativeCost = relativeCost
        };

        foreach (var col in predicateColumns)
        {
            node.PredicateColumns.Add(col);
        }

        foreach (var col in outputColumns)
        {
            node.OutputColumns.Add(col);
        }

        return node;
    }
}
