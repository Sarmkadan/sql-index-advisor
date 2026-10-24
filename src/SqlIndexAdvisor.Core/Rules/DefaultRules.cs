using SqlIndexAdvisor.Core.Engine;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Default rules for the recommendation engine.
/// </summary>
public static class DefaultRules
{
    /// <summary>
    /// Returns the default set of rules.
    /// </summary>
    /// <returns>The default set of rules.</returns>
    public static IReadOnlyList<IIndexRule> All() => All(new RecommendationOptions());

    /// <summary>
    /// Returns the default set of rules, with the row-count thresholds of the given options applied to the
    /// rules that use them.
    /// </summary>
    /// <param name="options">The table-size and selectivity thresholds for the scan rules.</param>
    /// <returns>The default set of rules.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<IIndexRule> All(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new List<IIndexRule>
        {
            new EngineHintRule(),
            new FullScanWithFilterRule(options),
            new ImplicitConversionRule(),
        };
    }
}
