using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Defines the contract for a recommendation engine that can analyze an execution plan
/// and produce a list of index recommendations.
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>
    /// Analyzes the supplied <paramref name="plan"/> and returns a read‑only list of
    /// <see cref="IndexRecommendation"/> objects.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>A read‑only list of index recommendations.</returns>
    IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan);
}
