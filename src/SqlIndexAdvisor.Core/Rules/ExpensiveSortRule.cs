using SqlIndexAdvisor.Core.Model;

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
    // A sort cheaper than this share of the statement isn't worth an index.
    private const double MinRelativeCost = 0.15;

    // If the sort is responsible for most of the cost, mark it High confidence.
    private const double HighConfidenceThreshold = 0.50;

    protected override bool ShouldVisit(PlanNode node)
    {
        return IsSortOperation(node) && node.RelativeCost >= MinRelativeCost;
    }

    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        // Extract columns from the Sort operation
        var sortColumns = ExtractSortColumns(node);
        if (sortColumns.Count == 0)
            yield break;

        // Determine which table this sort is operating on (if any)
        var tableName = DetermineTableForSort(node, GetParentChain(node));
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

    private static bool IsSortOperation(PlanNode node)
    {
        return node.Operator.StartsWith("Sort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.StartsWith("TopSort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.Equals("TopN Sort", StringComparison.OrdinalIgnoreCase) ||
               node.Operator.Equals("Stream Aggregate", StringComparison.OrdinalIgnoreCase) &&
               node.OutputColumns.Count > 0; // Stream Aggregate often indicates grouping/sorting work
    }

    private static List<string> ExtractSortColumns(PlanNode node)
    {
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

    private static string DetermineTableForSort(PlanNode node, IEnumerable<PlanNode> parentChain)
    {
        // Try to find the table this sort is operating on
        // Look at the node's table if available
        if (!string.IsNullOrEmpty(node.TableName))
            return node.TableName!;

        // Walk up the tree to find a scan node that feeds into this sort
        foreach (var parent in parentChain)
        {
            if (!string.IsNullOrEmpty(parent.TableName))
                return parent.TableName!;

            // If we hit another sort/top operation, keep looking
            if (parent.Operator.StartsWith("Sort", StringComparison.OrdinalIgnoreCase) ||
                parent.Operator.StartsWith("Top", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
        }

        // If no table found, use a generic name based on the operator
        return node.Operator.Split(' ')[0];
    }

    private static List<string> BuildIncludeColumns(PlanNode node, List<string> keyColumns)
    {
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
    private static double EstimateImpact(PlanNode node)
    {
        var baseline = node.RelativeCost * 100.0;
        var rowFactor = Math.Clamp(node.EstimatedRowsRead / Math.Max(node.EstimatedRows, 1), 1.0, 10.0);
        // Blend: even a small sort can be worth eliminating
        var factor = 0.3 + 0.7 * Math.Min(rowFactor, 5.0) / 5.0;
        return Math.Round(baseline * factor, 1);
    }
}