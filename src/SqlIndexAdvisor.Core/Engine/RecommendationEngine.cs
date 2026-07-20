using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Runs every rule against a plan, then de-duplicates. Two recommendations are
/// considered the same index if they target the same table with the same key
/// columns in the same order (a prefix match is treated as a dup - the wider one
/// wins and absorbs the other's includes/reasons).
/// </summary>
public sealed class RecommendationEngine
{
    private readonly IReadOnlyList<IIndexRule> _rules;

    public RecommendationEngine()
        : this(new IIndexRule[] { new EngineHintRule(), new FullScanWithFilterRule(), new ExpensiveSortRule() })
    {
    }

    public RecommendationEngine(IEnumerable<IIndexRule> rules) => _rules = rules.ToList();

    public IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan)
    {
        var raw = _rules.SelectMany(r => r.Evaluate(plan)).ToList();
        var merged = RecommendationMerger.Merge(raw);
        return merged
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.EstimatedImpactPercent)
            .ToList();
    }
}
