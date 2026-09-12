using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Evaluates execution plans with a collection of index rules and returns ranked,
/// de-duplicated index recommendations.
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
    /// <param name="rules">
    /// The index rules to evaluate against each execution plan. Null entries are ignored.
    /// </param>
    public RecommendationEngine(IEnumerable<IIndexRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.Where(r => r != null).ToList();
    }

    /// <summary>
    /// Analyzes an execution plan by evaluating every configured rule and merging
    /// duplicate index recommendations.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>
    /// A read-only list of merged recommendations ordered by descending confidence,
    /// then by descending estimated impact.
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
