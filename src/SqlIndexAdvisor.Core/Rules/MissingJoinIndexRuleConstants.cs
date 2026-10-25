namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Holds constant values used by <see cref="MissingJoinIndexRule"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class MissingJoinIndexRuleConstants
{
    // Join operator markers used to recognise a scan's parent join.
    public const string NestedLoopsOperator = "Nested Loops";
    public const string HashMatchOperator = "Hash Match";
    public const string MergeJoinOperator = "Merge Join";

    // Minimum share of statement cost for a join's full scan to be worth an index.
    public const double MinRelativeCost = 0.10;

    // Relative-cost cut-offs that map a scan's cost share to a confidence level.
    public const double HighConfidenceRelativeCost = 0.60;
    public const double MediumConfidenceRelativeCost = 0.30;

    public const double PercentScale = 100.0;
    public const int ImpactDecimals = 1;

    // Reason text: table, predicate columns, join operator, cost share as a whole percent.
    public const string ReasonFormat =
        "Scan on {0} with join predicate on ({1}) is part of a {2} join and is ~{3:0}% of statement cost. " +
        "An index on the join predicate columns would eliminate the full scan.";
}
