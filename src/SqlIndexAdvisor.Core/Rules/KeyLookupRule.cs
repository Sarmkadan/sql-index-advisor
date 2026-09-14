using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects SQL Server key‑lookup / RID‑lookup patterns and recommends a covering
/// index that combines the seek equality columns with the columns required by
/// the lookup as INCLUDE columns.
/// </summary>
public sealed class KeyLookupRule : PlanNodeVisitorBase
{
    /// <summary>
    /// A lookup cheaper than this share of the statement isn't worth an index.
    /// </summary>
    private const double MinRelativeCost = 0.10;

    /// <summary>
    /// Determines whether the specified plan node should be visited by this rule.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>True if the node represents a key lookup or RID lookup with sufficient cost; otherwise, false.</returns>
    protected override bool ShouldVisit(PlanNode node)
    {
        // Look for a key‑lookup (or RID‑lookup) operator.
        return IsKeyLookup(node)
        && !string.IsNullOrEmpty(node.TableName)
        && node.Parent != null
        && node.RelativeCost >= MinRelativeCost;
    }

    /// <summary>
    /// Visits the specified plan node. This overload is not supported as key lookup analysis requires the full execution plan.
    /// </summary>
    /// <param name="node">The plan node to visit.</param>
    /// <returns>An empty enumerable; this method always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Key lookup analysis needs the whole plan; the plan-aware overload is used.</exception>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node) =>
        throw new NotSupportedException("Key lookup analysis needs the whole plan; the plan-aware overload is used.");

    /// <summary>
    /// Visits the specified plan node within the context of an execution plan to generate index recommendations for key lookup operations.
    /// </summary>
    /// <param name="node">The key lookup plan node to analyze.</param>
    /// <param name="plan">The complete execution plan containing the node.</param>
    /// <returns>An enumerable of index recommendations for the key lookup operation.</returns>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node, ExecutionPlan plan)
    {
        // In showplan the lookup hangs off a join (usually Nested Loops) whose
        // other input is the index seek that supplies the equality predicates.
        // Prefer that sibling seek; fall back to the join node's own predicate.
        var seek = plan.Nodes.FirstOrDefault(n =>
        !ReferenceEquals(n, node)
        && ReferenceEquals(n.Parent, node.Parent)
        && n.Operator.Contains("Seek", StringComparison.OrdinalIgnoreCase)
        && n.PredicateColumns.Count > 0);

        if (seek == null)
        {
            seek = node.Parent!;
        }

        var equalityColumns = seek.PredicateColumns;
        if (equalityColumns.Count == 0)
            yield break;

        // Columns output by the lookup that are not already part of the key become INCLUDE.
        var include = node.OutputColumns
        .Where(c => !equalityColumns.Contains(c))
        .ToList();

        var confidence = node.RelativeCost switch
        {
            >= 0.60 => Confidence.High,
            >= 0.30 => Confidence.Medium,
            _ => Confidence.Low
        };

        // Use the existing index name from the seek node if available, to suggest
        // widening that index with INCLUDE columns rather than creating a new index.
        var existingIndexName = seek.IndexName;

        yield return new IndexRecommendation
        {
            Table = node.TableName!,
            KeyColumns = equalityColumns.ToList(),
            IncludeColumns = include,
            ExistingIndexName = existingIndexName,
            EstimatedImpactPercent = Math.Round(node.RelativeCost * 100.0, 1),
            SourceNodeCost = node.RelativeCost,
            Confidence = confidence,
            Reasons = {
                $"{node.Operator} on {node.TableName} follows an index seek with equality on " +
                $"({string.Join(", ", equalityColumns)}) and then performs a lookup; a covering index could avoid the extra lookup."
            }
        };
    }

    /// <summary>
    /// Determines whether the specified plan node represents a key lookup or RID lookup operation.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>True if the node is a key lookup or RID lookup; otherwise, false.</returns>
    private static bool IsKeyLookup(PlanNode node)
    {
        var op = node.Operator;
        return op.Contains("Key Lookup", StringComparison.OrdinalIgnoreCase) ||
        op.Contains("RID Lookup", StringComparison.OrdinalIgnoreCase);
    }
}