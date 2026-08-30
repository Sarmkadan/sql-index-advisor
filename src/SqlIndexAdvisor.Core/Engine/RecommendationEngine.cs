using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Runs every rule against a plan, then de-duplicates. Two recommendations are
/// considered the same index if they target the same table with the same key
/// columns in the same order (a prefix match is treated as a dup - the wider one
/// wins and absorbs the other's includes/reasons).
/// </summary>
public sealed class RecommendationEngine : IRecommendationEngine
{
    private readonly IReadOnlyList<IIndexRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendationEngine"/> class
    /// using the default set of index rules.
    /// </summary>
    public RecommendationEngine()
        : this(DefaultRules.All())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendationEngine"/> class
    /// with a custom set of index rules.
    /// </summary>
    /// <param name="rules">The rules to evaluate against each execution plan.</param>
    public RecommendationEngine(IEnumerable<IIndexRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.Where(r => r != null).ToList();
    }

    /// <summary>
    /// Analyzes the specified execution plan by running every configured rule,
    /// then merges and de-duplicates the raw recommendations.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>
    /// The merged recommendations, ordered by confidence and then by estimated impact,
    /// both descending.
    /// </returns>
    public IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var raw = CollectRaw(plan);
        var merged = RecommendationMerger.Merge(raw);
        return Rank(merged);
    }

    private List<IndexRecommendation> CollectRaw(ExecutionPlan plan)
    {
        var raw = new List<IndexRecommendation>();
        foreach (var rule in _rules)
        {
            var ruleResults = rule.Evaluate(plan);
            if (ruleResults != null)
            {
                foreach (var rec in ruleResults)
                {
                    raw.Add(rec);
                }
            }
        }
        return raw;
    }

    private List<IndexRecommendation> Rank(IEnumerable<IndexRecommendation> recommendations)
    {
        return recommendations
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.EstimatedImpactPercent)
            .ToList();
    }
}
