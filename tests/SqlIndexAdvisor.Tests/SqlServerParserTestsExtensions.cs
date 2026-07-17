using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="SqlServerParserTests"/> that provide additional test utilities
/// for working with SQL Server execution plans and parser functionality.
/// </summary>
public static class SqlServerParserTestsExtensions
{
    /// <summary>
    /// Creates a test SQL Server execution plan XML string with the specified cost.
    /// </summary>
    /// <param name="_">The test instance (discarded).</param>
    /// <param name="cost">The estimated total cost value for the plan.</param>
    /// <returns>A SQL Server execution plan XML string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="cost"/> is negative.</exception>
    public static string CreateTestPlanXml(this SqlServerParserTests _, double cost)
    {
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost), "Cost must be non-negative");
        }

        return $"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
            $"<ShowPlanXML xmlns=\"http://schemas.microsoft.com/sqlserver/2004/07/showplan\">" +
            $"<BatchSequence><Batch><Statements>" +
            $"<StmtSimple StatementText=\"SELECT * FROM dbo.Orders WHERE Status='open'\" StatementSubTreeCost=\"" + cost.ToString(CultureInfo.InvariantCulture) + $"\">" +
            $"<QueryPlan>" +
            $"<MissingIndexes>" +
            $"<MissingIndexGroup Impact=\"80\">" +
            $"<MissingIndex Table=\"[Orders]\" Schema=\"[dbo]\">" +
            $"<ColumnGroup Usage=\"EQUALITY\"><Column Name=\"[Status]\" /></ColumnGroup>" +
            $"<ColumnGroup Usage=\"INCLUDE\"><Column Name=\"[Total]\" /></ColumnGroup>" +
            $"</MissingIndex>" +
            $"</MissingIndexGroup>" +
            $"</MissingIndexes>" +
            $"<RelOp PhysicalOp=\"Clustered Index Scan\" EstimateRows=\"1000\" EstimatedTotalSubtreeCost=\"" + cost.ToString(CultureInfo.InvariantCulture) + $"\">" +
            $"<OutputList><ColumnReference Table=\"[Orders]\" Column=\"Total\" /></OutputList>" +
            $"<IndexScan>" +
            $"<Object Schema=\"[dbo]\" Table=\"[Orders]\" Index=\"[PK_Orders]\" />" +
            $"<Predicate><ScalarOperator><ColumnReference Table=\"[Orders]\" Column=\"Status\" /></ScalarOperator></Predicate>" +
            $"</IndexScan>" +
            $"</RelOp>" +
            $"</QueryPlan>" +
            $"</StmtSimple>" +
            $"</Statements></Batch></BatchSequence>" +
            $"</ShowPlanXML>";
    }

    /// <summary>
    /// Parses a SQL Server execution plan and returns the root <see cref="ExecutionPlan"/> object.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="planXml">The SQL Server execution plan XML string.</param>
    /// <returns>The parsed execution plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="planXml"/> is null.</exception>
    public static ExecutionPlan ParsePlan(this SqlServerParserTests test, string planXml)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(planXml);

        return new SqlServerXmlPlanParser().Parse(planXml);
    }

    /// <summary>
    /// Gets the first node from a parsed execution plan.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="plan">The execution plan to extract the node from.</param>
    /// <returns>The first node in the plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the plan has no nodes.</exception>
    public static PlanNode GetFirstNode(this SqlServerParserTests test, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.Nodes.Count == 0)
        {
            throw new ArgumentException("Execution plan has no nodes", nameof(plan));
        }

        return plan.Nodes[0];
    }

    /// <summary>
    /// Creates a test SQL Server execution plan with a missing index hint for a specific table.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="tableName">The name of the table (without schema).</param>
    /// <param name="equalityColumns">Columns used in equality predicates.</param>
    /// <param name="includeColumns">Columns to include in the index.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <param name="impactPercent">The impact percentage for the missing index (0-100).</param>
    /// <returns>A SQL Server execution plan XML string with the specified missing index.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/>, <paramref name="equalityColumns"/>, or <paramref name="includeColumns"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> or <paramref name="schemaName"/> is empty.</exception>
    public static string CreatePlanWithMissingIndex(
        this SqlServerParserTests test,
        string tableName,
        IReadOnlyList<string> equalityColumns,
        IReadOnlyList<string> includeColumns,
        string schemaName = "dbo",
        int impactPercent = 80)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));
        ArgumentException.ThrowIfNullOrEmpty(schemaName, nameof(schemaName));
        ArgumentNullException.ThrowIfNull(equalityColumns, nameof(equalityColumns));
        ArgumentNullException.ThrowIfNull(includeColumns, nameof(includeColumns));

        if (impactPercent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(impactPercent), "Impact percent must be non-negative");
        }

        if (impactPercent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(impactPercent), "Impact percent must be <= 100");
        }

        var equalityColumnsXml = string.Join("", equalityColumns.Select(c => $"<Column Name=\"[{c}]\" />"));
        var includeColumnsXml = string.Join("", includeColumns.Select(c => $"<Column Name=\"[{c}]\" />"));

        return $"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
            $"<ShowPlanXML xmlns=\"http://schemas.microsoft.com/sqlserver/2004/07/showplan\">" +
            $"<BatchSequence><Batch><Statements>" +
            $"<StmtSimple StatementText=\"SELECT * FROM " + schemaName + "." + tableName + "\" StatementSubTreeCost=\"2.5\">" +
            $"<QueryPlan>" +
            $"<MissingIndexes>" +
            $"<MissingIndexGroup Impact=\"" + impactPercent + "\">" +
            $"<MissingIndex Table=\"[" + tableName + "]\" Schema=\"[" + schemaName + "]\">" +
            $"<ColumnGroup Usage=\"EQUALITY\">" + equalityColumnsXml + $"</ColumnGroup>" +
            $"<ColumnGroup Usage=\"INCLUDE\">" + includeColumnsXml + $"</ColumnGroup>" +
            $"</MissingIndex>" +
            $"</MissingIndexGroup>" +
            $"</MissingIndexes>" +
            $"<RelOp PhysicalOp=\"Clustered Index Scan\" EstimateRows=\"500\" EstimatedTotalSubtreeCost=\"2.5\">" +
            $"<OutputList><ColumnReference Table=\"[" + schemaName + "].[" + tableName + "]\" Column=\"Id\" /></OutputList>" +
            $"<IndexScan>" +
            $"<Object Schema=\"[" + schemaName + "]\" Table=\"[" + tableName + "]\" Index=\"[PK_" + tableName + "]\" />" +
            $"</IndexScan>" +
            $"</RelOp>" +
            $"</QueryPlan>" +
            $"</StmtSimple>" +
            $"</Statements></Batch></BatchSequence>" +
            $"</ShowPlanXML>";
    }
}
