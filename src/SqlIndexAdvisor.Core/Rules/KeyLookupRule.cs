using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects SQL Server key‑lookup / RID‑lookup patterns and recommends a covering
/// index that combines the seek equality columns with the columns required by
/// the lookup as INCLUDE columns.
/// </summary>
public sealed class KeyLookupRule : PlanNodeVisitorBase
{
    // A lookup cheaper than this share of the statement isn't worth an index.
    private const double MinRelativeCost = 0.10;

    protected override bool ShouldVisit(PlanNode node)
    {
        // Look for a key‑lookup (or RID‑lookup) operator.
        return IsKeyLookup(node)
            && !string.IsNullOrEmpty(node.TableName)
            && node.Parent != null
            && node.RelativeCost >= MinRelativeCost;
    }

    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        // The parent should be the index seek that supplies the equality predicates.
        var parent = node.Parent!;
        var equalityColumns = parent.PredicateColumns;
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

        yield return new IndexRecommendation
        {
            Table = node.TableName!,
            KeyColumns = equalityColumns.ToList(),
            IncludeColumns = include,
            EstimatedImpactPercent = Math.Round(node.RelativeCost * 100.0, 1),
            SourceNodeCost = node.RelativeCost,
            Confidence = confidence,
            Reasons = {
                $"{node.Operator} on {node.TableName} follows an index seek with equality on " +
                $"({string.Join(", ", equalityColumns)}) and then performs a lookup; a covering index could avoid the extra lookup."
            }
        };
    }

    private static bool IsKeyLookup(PlanNode node)
    {
        var op = node.Operator;
        return op.Contains("Key Lookup", StringComparison.OrdinalIgnoreCase) ||
               op.Contains("RID Lookup", StringComparison.OrdinalIgnoreCase);
    }
}