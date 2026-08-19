namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Holds constant values used by <see cref="IndexRecommendationExtensions"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class IndexRecommendationExtensionsConstants
{
    public const string ImpactPercentFormat = "F1";
    public const string ColumnSeparator = ", ";
    public const string IncludeColumnsPrefix = " INCLUDE (";
    public const string ClosingParenthesis = ")";
    public const string NoColumnsPlaceholder = "(none)";
    public const string IndexPrefix = "Index ";
    public const string OnTableSeparator = " on ";
    public const string ImpactLabel = "- Impact: ";
    public const string PercentSuffix = "% ";
    public const string ConfidenceLabel = "Confidence: ";
    public const string SummarySeparator = "- ";
    public const string ImpactPercentSuffix = "% impact";
}