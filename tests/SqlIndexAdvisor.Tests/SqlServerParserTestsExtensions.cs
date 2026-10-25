using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides extension methods for creating and parsing SQL Server execution plan test data.
/// </summary>
/// <remarks>
/// This class contains utility methods for generating test execution plans with various characteristics
/// (missing indexes, scan operations, costs) and parsing them into <see cref="ExecutionPlan"/> objects
/// for testing SQL Server parser functionality.
/// </remarks>
public static class SqlServerParserTestsExtensions
{
    /// <summary>
    /// Creates a test SQL Server execution plan XML string with the specified cost.
    /// </summary>
    /// <param name="_">The test instance.</param>
    /// <param name="cost">The estimated total cost value for the plan.</param>
    /// <returns>A SQL Server execution plan XML string with a clustered index scan operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="cost"/> is negative.</exception>
    public static string CreateTestPlanXml(this SqlServerParserTests _, double cost)
    {
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost), SqlServerParserTestsExtensionsConstants.NegativeCostMessage);
        }

        var costStr = cost.ToString(CultureInfo.InvariantCulture);
        return SqlServerParserTestsExtensionsConstants.XmlDeclaration +
               SqlServerParserTestsExtensionsConstants.ShowPlanOpenTag +
               SqlServerParserTestsExtensionsConstants.BatchSequenceOpenTag +
               $"<StmtSimple StatementText=\"{SqlServerParserTestsExtensionsConstants.OrdersStatementText}\" StatementSubTreeCost=\"" + costStr + $"\">" +
               $"<QueryPlan>" +
               $"<MissingIndexes>" +
               $"<MissingIndexGroup Impact=\"{SqlServerParserTestsExtensionsConstants.DefaultImpactPercent}\">" +
               $"<MissingIndex Table=\"[{SqlServerParserTestsExtensionsConstants.OrdersTableName}]\" Schema=\"[{SqlServerParserTestsExtensionsConstants.DefaultSchemaName}]\">" +
               $"<ColumnGroup Usage=\"{SqlServerParserTestsExtensionsConstants.EqualityUsage}\"><Column Name=\"[{SqlServerParserTestsExtensionsConstants.StatusColumnName}]\" /></ColumnGroup>" +
               $"<ColumnGroup Usage=\"{SqlServerParserTestsExtensionsConstants.IncludeUsage}\"><Column Name=\"[{SqlServerParserTestsExtensionsConstants.TotalColumnName}]\" /></ColumnGroup>" +
               $"</MissingIndex>" +
               $"</MissingIndexGroup>" +
               $"</MissingIndexes>" +
               $"<RelOp PhysicalOp=\"{SqlServerParserTestsExtensionsConstants.ClusteredIndexScanOperator}\" EstimateRows=\"{SqlServerParserTestsExtensionsConstants.OrdersTableEstimateRows}\" EstimatedTotalSubtreeCost=\"" + costStr + $"\">" +
               $"<OutputList><ColumnReference Table=\"[{SqlServerParserTestsExtensionsConstants.OrdersTableName}]\" Column=\"{SqlServerParserTestsExtensionsConstants.TotalColumnName}\" /></OutputList>" +
               $"<IndexScan>" +
               $"<Object Schema=\"[{SqlServerParserTestsExtensionsConstants.DefaultSchemaName}]\" Table=\"[{SqlServerParserTestsExtensionsConstants.OrdersTableName}]\" Index=\"[{SqlServerParserTestsExtensionsConstants.PrimaryKeyPrefix}{SqlServerParserTestsExtensionsConstants.OrdersTableName}]\" />" +
               $"<Predicate><ScalarOperator><ColumnReference Table=\"[{SqlServerParserTestsExtensionsConstants.OrdersTableName}]\" Column=\"{SqlServerParserTestsExtensionsConstants.StatusColumnName}\" /></ScalarOperator></Predicate>" +
               $"</IndexScan>" +
               $"</RelOp>" +
               $"</QueryPlan>" +
               $"</StmtSimple>" +
               SqlServerParserTestsExtensionsConstants.BatchSequenceCloseTag +
               SqlServerParserTestsExtensionsConstants.ShowPlanCloseTag;
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

        return new SqlServerXmlPlanParser().Parse(planXml).Single();
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

        return plan.Nodes.Count switch
        {
            0 => throw new ArgumentException(SqlServerParserTestsExtensionsConstants.NoNodesMessage, nameof(plan)),
            _ => plan.Nodes[0]
        };
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
        string schemaName = SqlServerParserTestsExtensionsConstants.DefaultSchemaName,
        int impactPercent = SqlServerParserTestsExtensionsConstants.DefaultImpactPercent)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));
        ArgumentException.ThrowIfNullOrEmpty(schemaName, nameof(schemaName));
        ArgumentNullException.ThrowIfNull(equalityColumns, nameof(equalityColumns));
        ArgumentNullException.ThrowIfNull(includeColumns, nameof(includeColumns));

        if (impactPercent is < SqlServerParserTestsExtensionsConstants.MinImpactPercent or > SqlServerParserTestsExtensionsConstants.MaxImpactPercent)
        {
            throw new ArgumentOutOfRangeException(nameof(impactPercent), impactPercent, SqlServerParserTestsExtensionsConstants.ImpactPercentRangeMessage);
        }

        var equalityColumnsXml = string.Join("", equalityColumns.Select(c => string.Format(CultureInfo.InvariantCulture, SqlServerParserTestsExtensionsConstants.ColumnElementTemplate, c)));
        var includeColumnsXml = string.Join("", includeColumns.Select(c => string.Format(CultureInfo.InvariantCulture, SqlServerParserTestsExtensionsConstants.ColumnElementTemplate, c)));

        return SqlServerParserTestsExtensionsConstants.XmlDeclaration +
               SqlServerParserTestsExtensionsConstants.ShowPlanOpenTag +
               SqlServerParserTestsExtensionsConstants.BatchSequenceOpenTag +
               $"<StmtSimple StatementText=\"{SqlServerParserTestsExtensionsConstants.FromClausePrefix}" + schemaName + "." + tableName + $"\" StatementSubTreeCost=\"{SqlServerParserTestsExtensionsConstants.DefaultSubtreeCost}\">" +
               $"<QueryPlan>" +
               $"<MissingIndexes>" +
               $"<MissingIndexGroup Impact=\"" + impactPercent + "\">" +
               $"<MissingIndex Table=\"[" + tableName + "]\" Schema=\"[" + schemaName + "]\">" +
               $"<ColumnGroup Usage=\"{SqlServerParserTestsExtensionsConstants.EqualityUsage}\">" +
               equalityColumnsXml +
               $"</ColumnGroup>" +
               $"<ColumnGroup Usage=\"{SqlServerParserTestsExtensionsConstants.IncludeUsage}\">" +
               includeColumnsXml +
               $"</ColumnGroup>" +
               $"</MissingIndex>" +
               $"</MissingIndexGroup>" +
               $"</MissingIndexes>" +
               $"<RelOp PhysicalOp=\"{SqlServerParserTestsExtensionsConstants.ClusteredIndexScanOperator}\" EstimateRows=\"{SqlServerParserTestsExtensionsConstants.IndexedTableEstimateRows}\" EstimatedTotalSubtreeCost=\"{SqlServerParserTestsExtensionsConstants.DefaultSubtreeCost}\">" +
               $"<OutputList><ColumnReference Table=\"[" + schemaName + "].[" + tableName + $"]\" Column=\"{SqlServerParserTestsExtensionsConstants.IdColumnName}\" /></OutputList>" +
               $"<IndexScan>" +
               $"<Object Schema=\"[" + schemaName + "]\" Table=\"[" + tableName + "]\" Index=\"[" + SqlServerParserTestsExtensionsConstants.PrimaryKeyPrefix + tableName + "]\" />" +
               $"</IndexScan>" +
               $"</RelOp>" +
               $"</QueryPlan>" +
               $"</StmtSimple>" +
               SqlServerParserTestsExtensionsConstants.BatchSequenceCloseTag +
               SqlServerParserTestsExtensionsConstants.ShowPlanCloseTag;
    }
}
