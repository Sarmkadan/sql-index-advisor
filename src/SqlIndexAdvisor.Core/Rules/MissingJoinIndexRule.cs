using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects nested-loop and hash join patterns where the inner side performs a full scan
/// with join predicate columns. These patterns indicate that an index on the join predicate
/// columns would eliminate the full scan and improve performance.
/// </summary>
public sealed class MissingJoinIndexRule : IIndexRule
{
    public string Name => "join-with-full-scan";

    // A join with full scan cheaper than this share of the statement isn't worth an index.
    private const double MinRelativeCost = 0.10;

    public IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        foreach (var node in plan.Nodes)
        {
            // Look for scan nodes that are part of join operations
            // These scans on the inner side of joins often have join predicate columns
            if (!node.IsScan)
                continue;

            if (string.IsNullOrEmpty(node.TableName))
                continue;

            // Ensure we have join predicate columns (columns used in join conditions)
            if (node.PredicateColumns.Count == 0)
                continue;

            // Check if this scan is part of a join operation by looking at its parent
            var parent = node.Parent;
            if (parent == null)
                continue;

            // Check if parent is a join operator
            var parentOp = parent.Operator;
            var isJoinOperator = parentOp.Contains("Nested Loops", StringComparison.OrdinalIgnoreCase) ||
                               parentOp.Contains("Hash Match", StringComparison.OrdinalIgnoreCase) ||
                               parentOp.Contains("Merge Join", StringComparison.OrdinalIgnoreCase);

            if (!isJoinOperator)
                continue;

            if (node.RelativeCost < MinRelativeCost)
                continue;

            // The output columns that aren't part of the join predicate become INCLUDE candidates
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
                EstimatedImpactPercent = Math.Round(node.RelativeCost * 100.0, 1),
                Confidence = confidence,
                Reasons = new List<string>
                {
                    $"Scan on {node.TableName} with join predicate on " +
                    $"({string.Join(", ", node.PredicateColumns)}) is part of a {parentOp} join " +
                    $"and is ~{node.RelativeCost * 100:0}% of statement cost. " +
                    "An index on the join predicate columns would eliminate the full scan."
                }
            };
        }
    }
}