namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Holds constant values used by <see cref="EngineHintRule"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class EngineHintRuleConstants
{
    public const string RuleName = "engine-hint";
    public const double PercentScale = 100.0;
    public const string ImpactReasonFormat = "Optimizer reported a missing index with {0:0.#}% estimated impact.";
}
