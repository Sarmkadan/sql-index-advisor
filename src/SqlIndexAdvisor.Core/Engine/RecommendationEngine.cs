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
        : this(DefaultRules.All())
    {
    }

    public RecommendationEngine(IEnumerable<IIndexRule> rules) => _rules = rules.ToList();

    public IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var raw = new List<IndexRecommendation>();

        // Single-pass traversal: visit each node once and dispatch to all rules
        // This reduces complexity from O(rules × nodes) to O(nodes) for PlanNodeVisitorBase rules
        foreach (var rule in _rules)
        {
            foreach (var rec in rule.Evaluate(plan))
            {
                raw.Add(rec);
            }
        }

        var merged = RecommendationMerger.Merge(raw);
        return merged
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.EstimatedImpactPercent)
            .ToList();
    }
}
