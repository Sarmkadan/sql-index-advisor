using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Defines a service that analyzes a database execution plan and identifies indexes
/// that may improve the performance of the represented statement.
/// </summary>
/// <remarks>
/// Implementations are responsible for applying their configured analysis rules and
/// returning the resulting recommendations. Callers should treat the returned
/// recommendations as advisory and validate them against their workload before
/// changing a database schema.
/// </remarks>
public interface IRecommendationEngine
{
    /// <summary>
    /// Analyzes <paramref name="plan"/> and produces the index recommendations found
    /// by the engine.
    /// </summary>
    /// <param name="plan">
    /// The parsed execution plan to inspect. It must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A read-only list of recommendations. The list is empty when the engine finds
    /// no applicable index improvements.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="plan"/> is <see langword="null"/>.
    /// </exception>
    IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan);
}
