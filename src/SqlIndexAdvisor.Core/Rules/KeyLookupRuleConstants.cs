namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Holds constant values used by <see cref="KeyLookupRule"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class KeyLookupRuleConstants
{
    // Operator names as they appear in SQL Server showplan output.
    public const string KeyLookupOperator = "Key Lookup";
    public const string RidLookupOperator = "RID Lookup";
    public const string SeekOperatorMarker = "Seek";

    // Minimum share of statement cost for a lookup to be worth an index.
    public const double MinRelativeCost = 0.10;

    // Relative-cost cut-offs that map a lookup's cost share to a confidence level.
    public const double HighConfidenceRelativeCost = 0.60;
    public const double MediumConfidenceRelativeCost = 0.30;

    public const double PercentScale = 100.0;
    public const int ImpactDecimals = 1;

    // Reason text: operator, table, then the seek's equality columns.
    public const string ReasonFormat =
        "{0} on {1} follows an index seek with equality on ({2}) and then performs a lookup; a covering index could avoid the extra lookup.";
}
