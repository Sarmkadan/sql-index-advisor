using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="ExecutionPlan"/> that provide additional utility
/// for working with execution plans in test scenarios.
/// </summary>
public static class ExecutionPlanExtensionsTestsExtensions
{
    /// <summary>
    /// Gets the scan nodes from the execution plan that have the highest estimated rows read.
    /// </summary>
    /// <param name="plan">The execution plan to analyze. Must not be null.</param>
    /// <returns>An enumerable of scan nodes with the highest estimated rows read, or empty if no scans exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<PlanNode> GetHighestRowScanNodes(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.Nodes is null || !plan.Nodes.Any())
        {
            return Enumerable.Empty<PlanNode>();
        }

        var maxRowsRead = plan.Nodes
            .Where(node => node.IsScan)
            .Max(node => node.EstimatedRowsRead);

        return plan.Nodes
            .Where(node => node.IsScan && node.EstimatedRowsRead == maxRowsRead)
            .ToList();
    }

    /// <summary>
    /// Gets the total estimated rows read across all scan operations in the execution plan.
    /// </summary>
    /// <param name="plan">The execution plan to analyze. Must not be null.</param>
    /// <returns>The sum of estimated rows read for all scan operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static double GetTotalScanRowsRead(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan)
            .Sum(node => node.EstimatedRowsRead);
    }

    /// <summary>
    /// Gets the distinct table names that have scan operations with high estimated rows read.
    /// Tables with scans reading more than 1000 rows are considered "high volume".
    /// </summary>
    /// <param name="plan">The execution plan to analyze. Must not be null.</param>
    /// <returns>An enumerable of distinct table names with high-volume scans.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetHighVolumeScannedTables(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan && node.EstimatedRowsRead > 1000)
            .Select(node => node.TableName)
            .Where(tableName => !string.IsNullOrEmpty(tableName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Gets the scan operations that could benefit most from a covering index.
    /// These are scans where the predicate columns are not already covered by the index.
    /// </summary>
    /// <param name="plan">The execution plan to analyze. Must not be null.</param>
    /// <returns>An enumerable of scan nodes that would benefit from covering indexes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<PlanNode> GetScansNeedingCoveringIndex(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan && node.PredicateColumns.Count > 0)
            .Where(node => node.OutputColumns.Count(col => node.PredicateColumns.Contains(col, StringComparer.OrdinalIgnoreCase)) < node.PredicateColumns.Count)
            .ToList();
    }
}