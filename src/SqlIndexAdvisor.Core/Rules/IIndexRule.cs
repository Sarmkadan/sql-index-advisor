using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// A rule inspects a plan and emits zero or more raw recommendations. Rules are
/// independent; overlap is resolved later by <see cref="Engine.RecommendationEngine"/>.
/// </summary>
public interface IIndexRule
{
    string Name { get; }
    IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan);
}
