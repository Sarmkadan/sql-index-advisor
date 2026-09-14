using SqlIndexAdvisor.Core.Model;
using System;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Flags Sort operations that carry a high fraction of the statement cost.
/// Sort operations that read many rows and emit many rows are expensive; an
/// index on the sort columns (or a subset) can turn the sort into an index-ordered
/// scan or eliminate it entirely. The rule recommends an index on the columns
/// referenced in ORDER BY (or the top-level Sort's columns) and includes any
/// additional columns the query needs so the index can cover it.
/// </summary>
public sealed class ExpensiveSortRule : PlanNodeVisitorBase
{
    /// <summary>
    /// A sort cheaper than this share of the statement isn't worth an index.
    /// </summary>
    private const double MinRelativeCost = 0.15;

    /// <summary>
    /// If the sort is responsible for most of the cost, mark it High confidence.
    /// </summary>
    private const double HighConfidenceThreshold = 0.50;

    /// <summary>
    /// Determines whether the specified plan node should be visited by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>True if the node represents a sort operation with sufficient cost; otherwise, false.</returns>
    protected override bool ShouldVisit(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return IsSortOperation(node) && node.RelativeCost >= MinRelativeCost;
    }

    /// <summary>
    /// Visits the specified plan node. This overload is not supported as sort analysis requires the full execution plan.
    /// </summary>
    /// <param name="node">The plan node to visit.</param>
    /// <returns>An empty enumerable; this method always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Sort analysis requires the full execution plan; use the plan-aware overload.</exception>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        throw new NotSupportedException("Sort analysis needs the whole plan; the plan-aware overload is used.");
    }

    /// <summary>
    /// Visits the specified plan node within the context of an execution plan to generate index recommendations for expensive sort operations.
    /// </summary>
    /// <param name="node">The sort operation plan node to analyze.</param>
    /// <param name="plan">The complete execution plan containing the node.</param>
    /// <returns>An enumerable of index recommendations for the sort operation.</returns>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(plan);

        // Extract columns from the Sort operation
        var sortColumns = ExtractSortColumns(node);
        if (sortColumns.Count == 0)
            yield break;

        // Determine which table this sort is operating on (if any)
        var tableName = DetermineTableForSort(node, plan);
        if (string.IsNullOrEmpty(tableName))
            yield break;

        // Build include columns from the output of this node
        var includeColumns = BuildIncludeColumns(node, sortColumns);

        var confidence = node.RelativeCost switch
        {
            >= HighConfidenceThreshold => Confidence.High,
            >= MinRelativeCost => Confidence.Medium,
            _ => Confidence.Low
        };

        yield return new IndexRecommendation
        {
            Table = tableName,
            KeyColumns = sortColumns,
            IncludeColumns = includeColumns,
            EstimatedImpactPercent = EstimateImpact(node),
            SourceNodeCost = node.RelativeCost,
            Confidence = confidence,
            Reasons = new List<string>
            {
                $"Sort operation ({node.Operator}) on {tableName} is ~{node.RelativeCost * 100:0}% of statement cost. " +
                $"Creating an index on ({string.Join(", ", sortColumns)}) can eliminate or reduce this expensive sort."
            }
        };
    }

    /// <summary>
    /// Determines whether the specified plan node represents a sort operation that should be analyzed by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>True if the node is a sort operation; otherwise, false.</returns>
    private static bool IsSortOperation(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Operator.StartsWith("Sort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.StartsWith("TopSort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.Equals("TopN Sort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.Equals("Stream Aggregate", StringComparison.OrdinalIgnoreCase) &&
               node.OutputColumns.Count > 0; // Stream Aggregate often indicates grouping/sorting work
    }

    /// <summary>
    /// Extracts the sort columns from the specified plan node.
    /// </summary>
    /// <param name="node">The plan node containing sort operation details.</param>
    /// <returns>A list of column names that are being sorted.</returns>
    private static List<string> ExtractSortColumns(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var columns = new List<string>();

        // For Sort operations, the columns are typically in OutputColumns
        // For Stream Aggregate, look at the grouping columns
        if (node.Operator.StartsWith("Sort", StringComparison.OrdinalIgnoreCase) ||
            node.Operator.StartsWith("TopSort", StringComparison.OrdinalIgnoreCase))
        {
            // Sort operations output the columns they sort by
            columns.AddRange(node.OutputColumns);
        }
        else if (node.Operator.Equals("Stream Aggregate", StringComparison.OrdinalIgnoreCase))
        {
            // Stream Aggregate has grouping columns that act like sort keys
            // These are typically in PredicateColumns or we need to infer from context
            // For now, use PredicateColumns which often contain the grouping columns
            columns.AddRange(node.PredicateColumns);
        }

        // Remove duplicates and ensure we have valid column names
        return columns.Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();
    }

    /// <summary>
    /// Determines the table that the sort operation is operating on, if it can be unambiguously identified.
    /// </summary>
    /// <param name="node">The sort operation plan node.</param>
    /// <param name="plan">The execution plan containing the node.</param>
    /// <returns>The name of the table being sorted, or null if the sort spans multiple tables or cannot be determined.</returns>
    private static string? DetermineTableForSort(PlanNode node, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(plan);

        // Try to find the table this sort is operating on
        // Look at the node's table if available
        if (!string.IsNullOrEmpty(node.TableName))
            return node.TableName!;

        // The rows being sorted come from below the sort in the tree. If exactly
        // one table feeds the sort we can attribute the index to it; if the sort
        // consumes several tables (e.g. above a join) we cannot pick one honestly.
        var tables = GetDescendants(node, plan)
            .Where(d => !string.IsNullOrEmpty(d.TableName))
            .Select(d => d.TableName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return tables.Count == 1 ? tables[0] : null;
    }

    /// <summary>
    /// Builds the list of include columns for an index recommendation based on the sort operation's output.
    /// </summary>
    /// <param name="node">The sort operation plan node.</param>
    /// <param name="keyColumns">The columns that form the key of the recommended index.</param>
    /// <returns>A list of column names to include in the index to cover the query.</returns>
    private static List<string> BuildIncludeColumns(PlanNode node, List<string> keyColumns)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(keyColumns);

        // Include columns are output columns that aren't already in the key
        var include = node.OutputColumns
            .Where(c => !keyColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return include;
    }

    /// <summary>
    /// Rough impact: sorts are O(n log n) in the number of rows, so we scale
    /// the node's cost share by how many rows it processes. A sort on many rows
    /// recovers more when converted to an index-ordered scan.
    /// </summary>
    /// <param name="node">The plan node representing the sort operation.</param>
    /// <returns>An estimated impact percentage for adding an index to eliminate this sort.</returns>
    private static double EstimateImpact(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var baseline = node.RelativeCost * 100.0;
        var rowFactor = Math.Clamp(node.EstimatedRowsRead / Math.Max(node.EstimatedRows, 1), 1.0, 10.0);
        // Blend: even a small sort can be worth eliminating
        var factor = 0.3 + 0.7 * Math.Min(rowFactor, 5.0) / 5.0;
        return Math.Round(baseline * factor, 1);
    }
}
