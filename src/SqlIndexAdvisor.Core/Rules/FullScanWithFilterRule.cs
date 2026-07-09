using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// The workhorse rule. Flags a table scan (SQL Server "Table Scan"/"Clustered
/// Index Scan", Postgres "Seq Scan") that carries a filter predicate and is
/// responsible for a meaningful chunk of the statement cost. The filtered
/// columns become the index key; the scan's output columns become INCLUDE
/// candidates so the index can cover the query.
/// </summary>
public sealed class FullScanWithFilterRule : IIndexRule
{
    public string Name => "scan-with-filter";

    // A scan cheaper than this share of the statement isn't worth an index.
    private const double MinRelativeCost = 0.10;

    public IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        foreach (var node in plan.Nodes)
        {
            if (!LooksLikeFullScan(node)) continue;
            if (string.IsNullOrEmpty(node.TableName)) continue;
            if (node.PredicateColumns.Count == 0) continue;
            if (node.RelativeCost < MinRelativeCost) continue;

            var include = node.OutputColumns
                .Where(c => !node.PredicateColumns.Contains(c))
                .ToList();

            var confidence = node.RelativeCost switch
            {
                >= 0.60 => Confidence.High,
                >= 0.30 => Confidence.Medium,
                _ => Confidence.Low
            };

            yield return new IndexRecommendation
            {
                Table = node.TableName!,
                KeyColumns = node.PredicateColumns.ToList(),
                IncludeColumns = include,
                EstimatedImpactPercent = EstimateImpact(node),
                Confidence = confidence,
                Reasons =
                {
                    $"{node.Operator} on {node.TableName} carries a filter on " +
                    $"({string.Join(", ", node.PredicateColumns)}) and is ~{node.RelativeCost * 100:0}% of statement cost."
                }
            };
        }
    }

    private static bool LooksLikeFullScan(PlanNode node)
    {
        var op = node.Operator;
        return op.Equals("Seq Scan", StringComparison.OrdinalIgnoreCase)
            || op.Equals("Table Scan", StringComparison.OrdinalIgnoreCase)
            || op.Equals("Clustered Index Scan", StringComparison.OrdinalIgnoreCase)
            || op.Equals("Index Scan", StringComparison.OrdinalIgnoreCase) && node.PredicateColumns.Count > 0;
    }

    /// <summary>
    /// Rough impact: most of a scan's cost is the rows it churns through, so we
    /// scale the node's cost share by how selective the filter looks. A very
    /// selective filter (few output rows relative to read rows) recovers more.
    /// </summary>
    private static double EstimateImpact(PlanNode node)
    {
        var baseline = node.RelativeCost * 100.0;
        var selectivity = node.EstimatedRowsRead > 0
            ? Math.Clamp(1.0 - node.EstimatedRows / node.EstimatedRowsRead, 0.0, 1.0)
            : 0.5;
        // Blend: even a non-selective filter still saves something by seeking.
        var factor = 0.4 + 0.6 * selectivity;
        return Math.Round(baseline * factor, 1);
    }
}
