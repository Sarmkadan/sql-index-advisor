using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Trusts the missing-index hints the optimizer already emitted (SQL Server).
/// These are the highest-confidence source we have because the engine costed
/// them itself. Equality columns come first, then inequality, matching the
/// standard leading-column ordering guidance.
/// </summary>
public sealed class EngineHintRule : PlanNodeVisitorBase
{
    public override string Name => "engine-hint";

    protected override bool ShouldVisit(PlanNode node) => false; // Engine hints don't visit nodes

    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node) => Array.Empty<IndexRecommendation>();

    public override IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        var recommendations = new List<IndexRecommendation>();

        foreach (var hint in plan.EngineMissingIndexes)
        {
            var keys = new List<string>();
            keys.AddRange(hint.EqualityColumns);
            keys.AddRange(hint.InequalityColumns.Where(c => !keys.Contains(c)));
            if (keys.Count == 0)
                continue;

            recommendations.Add(new IndexRecommendation
            {
                Table = hint.Table,
                KeyColumns = keys,
                IncludeColumns = hint.IncludeColumns.Where(c => !keys.Contains(c)).ToList(),
                EstimatedImpactPercent = hint.ImpactPercent,
                SourceNodeCost = hint.ImpactPercent / 100.0,
                Confidence = Confidence.High,
                Reasons = {
                    $"Optimizer reported a missing index with {hint.ImpactPercent:0.#}% estimated impact."
                }
            });
        }

        return recommendations;
    }
}
