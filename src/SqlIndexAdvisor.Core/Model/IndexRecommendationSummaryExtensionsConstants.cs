namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Holds constant values used by <see cref="IndexRecommendationSummaryExtensions"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class IndexRecommendationSummaryExtensionsConstants
{
    public const string DefaultReasonSeparator = "; ";
    public const string NoReasonsPlaceholder = "(no reasons)";
}
