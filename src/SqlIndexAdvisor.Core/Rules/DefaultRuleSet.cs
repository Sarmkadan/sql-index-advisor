using System.Collections.Generic;
using SqlIndexAdvisor.Core.Engine;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Provides the default collection of index‑recommendation rules used by the
/// engine. Adding a new rule here makes it part of the standard evaluation
/// pipeline without any further configuration.
/// </summary>
public static class DefaultRuleSet
{
    /// <summary>
    /// The default rule set: engine-emitted missing-index hints first (highest
    /// confidence), then the heuristic rules over the plan tree.
    /// </summary>
    public static IEnumerable<IIndexRule> Rules => Create(new RecommendationOptions());

    /// <summary>
    /// Creates the default rule set with the row-count thresholds of the given options applied to the
    /// scan rules that use them.
    /// </summary>
    /// <param name="options">The table-size and selectivity thresholds for the scan rules.</param>
    /// <returns>The default rules, in evaluation order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IEnumerable<IIndexRule> Create(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new IIndexRule[]
        {
            new EngineHintRule(),
            new FullScanWithFilterRule(options),
            new KeyLookupRule(),
            new ImplicitConversionRule(),
            new MissingJoinIndexRule(options),
            new ExpensiveSortRule(),
        };
    }
}
