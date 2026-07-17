using System;
using System.Collections.Generic;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods that make writing <see cref="PostgresParserTests"/> unit tests more concise.
/// </summary>
public static class PostgresParserTestsExtensions
{
    /// <summary>
    /// Parses a JSON execution plan using <see cref="PostgresJsonPlanParser"/>.
    /// </summary>
    /// <param name="_">The test instance (unused, enables fluent syntax).</param>
    /// <param name="json">The raw JSON plan string.</param>
    /// <returns>An <see cref="ExecutionPlan"/> representing the parsed plan.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="PlanParseException">The parser fails to parse the supplied JSON.</exception>
    public static ExecutionPlan ParsePlan(this PostgresParserTests _, string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return new PostgresJsonPlanParser().Parse(json);
    }

    /// <summary>
    /// Retrieves the single <see cref="PlanNode"/> with the specified <c>Operator</c> name from an <see cref="ExecutionPlan"/>.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <param name="plan">The execution plan to search.</param>
    /// <param name="operatorName">The operator name to match (e.g., <c>"Seq Scan"</c>).</param>
    /// <returns>The matching <see cref="PlanNode"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="plan"/> or <paramref name="operatorName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">No node or multiple nodes match the supplied operator name.</exception>
    public static PlanNode GetNodeByOperator(this PostgresParserTests _, ExecutionPlan plan, string operatorName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrEmpty(operatorName);
        return plan.Nodes.Single(node => node.Operator == operatorName);
    }

    /// <summary>
    /// Returns the predicate columns of a <see cref="PlanNode"/> as a read‑only list.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <param name="node">The plan node whose predicate columns are required.</param>
    /// <returns>A read‑only list of column names referenced in the node's predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetPredicateColumns(this PostgresParserTests _, PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        // The model already exposes PredicateColumns as an IReadOnlyList<string>.
        return node.PredicateColumns;
    }

    /// <summary>
    /// Asserts that a <see cref="PlanNode"/> contains all of the expected predicate columns.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <param name="node">The node to examine.</param>
    /// <param name="expectedColumns">The column names that must be present.</param>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="expectedColumns"/> is <c>null</c>.</exception>
    /// <exception cref="Xunit.Sdk.ContainsException">Any of the expected columns are missing.</exception>
    public static void AssertContainsPredicateColumns(this PostgresParserTests _, PlanNode node, params string[] expectedColumns)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(expectedColumns);
        foreach (var column in expectedColumns)
        {
            Xunit.Assert.Contains(column, node.PredicateColumns);
        }
    }
}
