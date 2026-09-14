using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// The workhorse rule. Flags a table scan (SQL Server "Table Scan"/"Clustered
/// Index Scan", Postgres "Seq Scan") that carries a filter predicate and is
/// responsible for a meaningful chunk of the statement cost. The filtered
/// columns become the index key; the scan's output columns become INCLUDE
/// candidates so the index can cover the query.
/// </summary>
public sealed class FullScanWithFilterRule : PlanNodeVisitorBase
{
    // A scan cheaper than this share of the statement isn't worth an index.
    private const double MinRelativeCost = 0.10;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullScanWithFilterRule"/> class.
    /// </summary>
    public FullScanWithFilterRule()
    {
    }

    /// <summary>
    /// Determines whether the specified plan node should be visited by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>
    ///   <c>true</c> if the node represents a full scan with a filter predicate and sufficient relative cost; otherwise, <c>false</c>.
    /// </returns>
    protected override bool ShouldVisit(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return LooksLikeFullScan(node)
            && !string.IsNullOrEmpty(node.TableName)
            && node.PredicateColumns.Count > 0
            && node.RelativeCost >= MinRelativeCost;
    }

    /// <summary>
/// Visits the specified plan node to generate index recommendations for full scans with filter predicates.
/// </summary>
/// <param name="node">The plan node representing a full scan operation with filter predicates.</param>
/// <returns>An enumerable of index recommendations for the full scan with filter operation.</returns>
protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
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
            SourceNodeCost = node.RelativeCost,
            Confidence = confidence,
            Rule = Name,
            Reasons = {
                $"{node.Operator} on {node.TableName} carries a filter on " +
                $"({string.Join(", ", node.PredicateColumns)}) and is ~{node.RelativeCost * 100:0}% of statement cost."
            }
        };
    }

    /// <summary>
/// Determines whether the specified plan node represents a full scan operation that should be considered for indexing.
/// </summary>
/// <param name="node">The plan node to evaluate.</param>
/// <returns>
///   <c>true</c> if the node represents a sequential scan, table scan, clustered index scan, or an index scan with filter predicates; otherwise, <c>false</c>.
/// </returns>
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
    /// <param name="node">The plan node representing the scan operation.</param>
    /// <returns>An estimated impact percentage for adding an index based on the scan's cost and filter selectivity.</returns>
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
