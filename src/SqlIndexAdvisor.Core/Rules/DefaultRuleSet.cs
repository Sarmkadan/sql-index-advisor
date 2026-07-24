using System.Collections.Generic;

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
    public static IEnumerable<IIndexRule> Rules => new IIndexRule[]
    {
        new EngineHintRule(),
        new FullScanWithFilterRule(),
        new KeyLookupRule(),
        new ImplicitConversionRule(),
        new MissingJoinIndexRule(),
        new ExpensiveSortRule(),
    };
}
