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
    /// <summary>
    /// Gets the name of the rule.
    /// </summary>
    public override string Name => "engine-hint";

    /// <summary>
    /// Determines whether the rule should visit the specified plan node.
    /// Engine hints do not visit nodes; they are processed at the plan level.
    /// </summary>
    /// <param name="node">The plan node to evaluate.</param>
    /// <returns>False, as engine hints are not node-specific.</returns>
    protected override bool ShouldVisit(PlanNode node) => false; // Engine hints don't visit nodes

    /// <summary>
    /// Visits the core plan node to generate index recommendations.
    /// Engine hints do not generate recommendations during node visitation.
    /// </summary>
    /// <param name="node">The plan node to visit.</param>
    /// <returns>An empty collection of index recommendations.</returns>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node) => Array.Empty<IndexRecommendation>();

    /// <summary>
    /// Evaluates the execution plan and generates index recommendations based on engine missing-index hints.
    /// </summary>
    /// <param name="plan">The execution plan to evaluate.</param>
    /// <returns>A collection of index recommendations derived from engine hints.</returns>
    public override IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
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
                Rule = Name,
                Reasons = {
                    $"Optimizer reported a missing index with {hint.ImpactPercent:0.#}% estimated impact."
                }
            });
        }

        return recommendations;
    }
}
