using SqlIndexAdvisor.Core.Rules;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Default rules for the recommendation engine.
/// </summary>
public static class DefaultRules
{
    /// <summary>
    /// Returns the default set of rules.
    /// </summary>
    public static IReadOnlyList<IIndexRule> All()
    {
        return new List<IIndexRule>
        {
            new EngineHintRule(),
            new ImplicitConversionRule(),
        };
    }
}
