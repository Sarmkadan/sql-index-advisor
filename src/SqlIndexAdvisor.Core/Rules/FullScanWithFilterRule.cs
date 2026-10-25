using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// The workhorse rule. Flags a table scan (SQL Server "Table Scan"/"Clustered
/// Index Scan", Postgres "Seq Scan") that carries a filter predicate and is
/// responsible for a meaningful chunk of the statement cost. The filtered
/// columns become the index key; the scan's output columns become INCLUDE
/// candidates so the index can cover the query. Scans on small tables, or whose
/// filter keeps most of the rows, are suppressed or downgraded using
/// <see cref="RecommendationOptions"/>.
/// </summary>
public sealed class FullScanWithFilterRule : PlanNodeVisitorBase
{
    private readonly RecommendationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullScanWithFilterRule"/> class with default thresholds.
    /// </summary>
    public FullScanWithFilterRule()
        : this(new RecommendationOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullScanWithFilterRule"/> class with the given thresholds.
    /// </summary>
    /// <param name="options">The table-size and selectivity thresholds to apply.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public FullScanWithFilterRule(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Determines whether the specified plan node should be visited by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>
    ///   <c>true</c> if the node represents a full scan with a filter predicate, sufficient relative cost,
    ///   and row counts that clear the table-size and selectivity thresholds; otherwise, <c>false</c>.
    /// </returns>
    protected override bool ShouldVisit(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return LooksLikeFullScan(node)
            && !string.IsNullOrEmpty(node.TableName)
            && node.PredicateColumns.Count > 0
            && node.RelativeCost >= FullScanWithFilterRuleConstants.MinRelativeCost
            && ScanRowStats.From(node).IsWorthIndexing(_options);
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

        var rows = ScanRowStats.From(node);
        var baseline = node.RelativeCost switch
        {
            >= FullScanWithFilterRuleConstants.HighConfidenceRelativeCost => Confidence.High,
            >= FullScanWithFilterRuleConstants.MediumConfidenceRelativeCost => Confidence.Medium,
            _ => Confidence.Low
        };

        yield return new IndexRecommendation
        {
            Table = node.TableName!,
            KeyColumns = node.PredicateColumns.ToList(),
            IncludeColumns = include,
            EstimatedImpactPercent = EstimateImpact(node, rows),
            SourceNodeCost = node.RelativeCost,
            Confidence = rows.AdjustConfidence(baseline, _options),
            Rule = Name,
            Reasons = [
                $"{node.Operator} on {node.TableName} carries a filter on " +
                $"({string.Join(", ", node.PredicateColumns)}) and is ~{node.RelativeCost * FullScanWithFilterRuleConstants.PercentScale:0}% of statement cost.",
                .. rows.Describe(_options)
            ]
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
    return op.Equals(FullScanWithFilterRuleConstants.SeqScanOperator, StringComparison.OrdinalIgnoreCase)
        || op.Equals(FullScanWithFilterRuleConstants.TableScanOperator, StringComparison.OrdinalIgnoreCase)
        || op.Equals(FullScanWithFilterRuleConstants.ClusteredIndexScanOperator, StringComparison.OrdinalIgnoreCase)
        || op.Equals(FullScanWithFilterRuleConstants.IndexScanOperator, StringComparison.OrdinalIgnoreCase) && node.PredicateColumns.Count > 0;
}

    /// <summary>
    /// Rough impact: most of a scan's cost is the rows it churns through, so we
    /// scale the node's cost share by how selective the filter looks. A very
    /// selective filter (few output rows relative to read rows) recovers more.
    /// When the plan has no row counts, a neutral 50% match is assumed.
    /// </summary>
    /// <param name="node">The plan node representing the scan operation.</param>
    /// <param name="rows">The resolved row counts for <paramref name="node"/>.</param>
    /// <returns>An estimated impact percentage for adding an index based on the scan's cost and filter selectivity.</returns>
    private static double EstimateImpact(PlanNode node, ScanRowStats rows)
    {
        var baseline = node.RelativeCost * FullScanWithFilterRuleConstants.PercentScale;
        var eliminated = 1.0 - (rows.MatchFraction ?? FullScanWithFilterRuleConstants.NeutralMatchFraction);
        // Blend: even a non-selective filter still saves something by seeking.
        var factor = FullScanWithFilterRuleConstants.SeekBaseFactor
            + FullScanWithFilterRuleConstants.SeekEliminationWeight * eliminated;
        return Math.Round(baseline * factor, FullScanWithFilterRuleConstants.ImpactDecimals);
    }
}
