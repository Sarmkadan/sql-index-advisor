using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Trusts the missing-index hints the optimizer already emitted (SQL Server).
/// These are the highest-confidence source we have because the engine costed
/// them itself. Equality columns come first, then inequality, matching the
/// standard leading-column ordering guidance.
/// </summary>
public sealed class EngineHintRule : IIndexRule
{
    public string Name => "engine-hint";

    public IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        foreach (var hint in plan.EngineMissingIndexes)
        {
            var keys = new List<string>();
            keys.AddRange(hint.EqualityColumns);
            keys.AddRange(hint.InequalityColumns.Where(c => !keys.Contains(c)));
            if (keys.Count == 0) continue;

            yield return new IndexRecommendation
            {
                Table = hint.Table,
                KeyColumns = keys,
                IncludeColumns = hint.IncludeColumns.Where(c => !keys.Contains(c)).ToList(),
                EstimatedImpactPercent = hint.ImpactPercent,
                Confidence = Confidence.High,
                Reasons = { $"Optimizer reported a missing index with {hint.ImpactPercent:0.#}% estimated impact." }
            };
        }
    }
}
