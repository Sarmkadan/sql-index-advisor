using SqlIndexAdvisor.Core.Model;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides extension methods for <see cref="ExecutionPlanTests"/> to simplify common test assertions and validations.
/// </summary>
public static class ExecutionPlanTestsExtensions
{
    /// <summary>
    /// Validates that the execution plan has the expected dialect.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedDialect">The expected dialect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasDialect(this ExecutionPlanTests tests, ExecutionPlan plan, PlanDialect expectedDialect)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        Assert.Equal(expectedDialect, plan.Dialect);
    }

    /// <summary>
    /// Validates that the execution plan has the expected statement text.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedStatementText">The expected statement text.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasStatementText(this ExecutionPlanTests tests, ExecutionPlan plan, string expectedStatementText)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(expectedStatementText);

        Assert.Equal(expectedStatementText, plan.StatementText);
    }

    /// <summary>
    /// Validates that the execution plan has the expected estimated total cost.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedCost">The expected estimated total cost.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasEstimatedTotalCost(this ExecutionPlanTests tests, ExecutionPlan plan, double expectedCost)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        Assert.Equal(expectedCost, plan.EstimatedTotalCost);
    }

    /// <summary>
    /// Validates that the execution plan has the expected number of nodes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedCount">The expected number of nodes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasNodesCount(this ExecutionPlanTests tests, ExecutionPlan plan, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        Assert.Equal(expectedCount, plan.Nodes.Count);
    }

    /// <summary>
    /// Validates that the execution plan has the expected number of missing indexes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedCount">The expected number of missing indexes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasMissingIndexesCount(this ExecutionPlanTests tests, ExecutionPlan plan, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        Assert.Equal(expectedCount, plan.EngineMissingIndexes.Count);
    }

    /// <summary>
    /// Validates that the execution plan has at least one scan operation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasScanOperations(this ExecutionPlanTests tests, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        var scanNodes = plan.Nodes.Where(n => n.IsScan).ToList();
        Assert.NotEmpty(scanNodes);
    }

    /// <summary>
    /// Validates that the execution plan has no scan operations.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasNoScanOperations(this ExecutionPlanTests tests, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        var scanNodes = plan.Nodes.Where(n => n.IsScan).ToList();
        Assert.Empty(scanNodes);
    }

    /// <summary>
    /// Validates that the execution plan contains a node with the specified operator.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="operatorName">The operator name to search for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/>, <paramref name="plan"/>, or <paramref name="operatorName"/> is <see langword="null"/>.</exception>
    public static void ContainsNodeWithOperator(this ExecutionPlanTests tests, ExecutionPlan plan, string operatorName)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(operatorName);

        var matchingNodes = plan.Nodes.Where(n => string.Equals(n.Operator, operatorName, StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(matchingNodes);
    }

    /// <summary>
    /// Validates that the execution plan contains a node with the specified table name.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="tableName">The table name to search for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/>, <paramref name="plan"/>, or <paramref name="tableName"/> is <see langword="null"/>.</exception>
    public static void ContainsNodeWithTable(this ExecutionPlanTests tests, ExecutionPlan plan, string tableName)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(tableName);

        var matchingNodes = plan.Nodes.Where(n => string.Equals(n.TableName, tableName, StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(matchingNodes);
    }

    /// <summary>
    /// Validates that the execution plan has a missing index with the specified impact percentage.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedImpactPercent">The expected impact percentage.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasMissingIndexWithImpact(this ExecutionPlanTests tests, ExecutionPlan plan, double expectedImpactPercent)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        var matchingIndex = plan.EngineMissingIndexes.FirstOrDefault(m => m.ImpactPercent.Equals(expectedImpactPercent));
        Assert.NotNull(matchingIndex);
    }

    /// <summary>
    /// Validates that the execution plan has a missing index for the specified table.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="tableName">The table name to search for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/>, <paramref name="plan"/>, or <paramref name="tableName"/> is <see langword="null"/>.</exception>
    public static void HasMissingIndexForTable(this ExecutionPlanTests tests, ExecutionPlan plan, string tableName)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(tableName);

        var matchingIndex = plan.EngineMissingIndexes.FirstOrDefault(m => string.Equals(m.Table, tableName, StringComparison.Ordinal));
        Assert.NotNull(matchingIndex);
    }

    /// <summary>
    /// Validates that the execution plan has nodes with estimated rows greater than zero.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasNodesWithEstimatedRows(this ExecutionPlanTests tests, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        var nodesWithRows = plan.Nodes.Where(n => n.EstimatedRows > 0).ToList();
        Assert.NotEmpty(nodesWithRows);
    }

    /// <summary>
    /// Validates that the execution plan has the expected total estimated rows across all nodes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="expectedTotalRows">The expected total estimated rows.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plan"/> is <see langword="null"/>.</exception>
    public static void HasTotalEstimatedRows(this ExecutionPlanTests tests, ExecutionPlan plan, double expectedTotalRows)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plan);

        var actualTotal = plan.Nodes.Sum(n => n.EstimatedRows);
        Assert.Equal(expectedTotalRows, actualTotal);
    }
}