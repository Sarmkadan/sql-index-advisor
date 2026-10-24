using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using System;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects nested-loop and hash join patterns where the inner side performs a full scan
/// with join predicate columns. These patterns indicate that an index on the join predicate
/// columns would eliminate the full scan and improve performance. Scans on small tables, or whose
/// join predicate keeps most of the rows, are suppressed or downgraded using <see cref="RecommendationOptions"/>.
/// </summary>
public sealed class MissingJoinIndexRule : PlanNodeVisitorBase
{
    /// <summary>
    /// A join with full scan cheaper than this share of the statement isn't worth an index.
    /// </summary>
    private const double MinRelativeCost = 0.10;

    private readonly RecommendationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingJoinIndexRule"/> class with default thresholds.
    /// </summary>
    public MissingJoinIndexRule()
        : this(new RecommendationOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingJoinIndexRule"/> class with the given thresholds.
    /// </summary>
    /// <param name="options">The table-size and selectivity thresholds to apply.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public MissingJoinIndexRule(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Determines whether the specified plan node should be visited by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>True if the node represents a scan with join predicate columns that is part of a join operation, meets the minimum cost threshold, and whose row counts clear the table-size and selectivity thresholds; otherwise, false.</returns>
    protected override bool ShouldVisit(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        // Look for scan nodes that are part of join operations
        // These scans on the inner side of joins often have join predicate columns
        return node.IsScan
            && !string.IsNullOrEmpty(node.TableName)
            && node.PredicateColumns.Count > 0
            && node.RelativeCost >= MinRelativeCost
            && IsJoinParent(node.Parent)
            && ScanRowStats.From(node).IsWorthIndexing(_options);
    }

    /// <summary>
    /// Visits the specified plan node to generate index recommendations for missing join indexes.
    /// </summary>
    /// <param name="node">The plan node representing a scan with join predicate columns that is part of a join operation.</param>
    /// <returns>An enumerable of index recommendations for the scan node.</returns>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        // The output columns that aren't part of the join predicate become INCLUDE candidates
        var include = node.OutputColumns
            .Where(c => !node.PredicateColumns.Contains(c))
            .ToList();

        var rows = ScanRowStats.From(node);
        var baseline = node.RelativeCost switch
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
            EstimatedImpactPercent = Math.Round(node.RelativeCost * 100.0, 1),
            SourceNodeCost = node.RelativeCost,
            Confidence = rows.AdjustConfidence(baseline, _options),
            Reasons =
            [
                $"Scan on {node.TableName} with join predicate on " +
                $"({string.Join(", ", node.PredicateColumns)}) is part of a {node.Parent!.Operator} join " +
                $"and is ~{node.RelativeCost * 100:0}% of statement cost. " +
                "An index on the join predicate columns would eliminate the full scan.",
                .. rows.Describe(_options)
            ]
        };
    }

    /// <summary>
    /// Determines whether the specified parent node represents a join operation (nested loops, hash match, or merge join).
    /// </summary>
    /// <param name="parent">The parent plan node to evaluate.</param>
    /// <returns>True if the parent node represents a join operation; otherwise, false.</returns>
    private static bool IsJoinParent(PlanNode? parent)
    {
        if (parent == null)
            return false;

        var parentOp = parent.Operator;
        return parentOp.Contains("Nested Loops", StringComparison.OrdinalIgnoreCase) ||
               parentOp.Contains("Hash Match", StringComparison.OrdinalIgnoreCase) ||
               parentOp.Contains("Merge Join", StringComparison.OrdinalIgnoreCase);
    }
}