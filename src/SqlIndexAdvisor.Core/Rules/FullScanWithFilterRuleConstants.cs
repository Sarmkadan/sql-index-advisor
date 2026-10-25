namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Holds constant values used by <see cref="FullScanWithFilterRule"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class FullScanWithFilterRuleConstants
{
    // Operator names as they appear in SQL Server showplan and Postgres EXPLAIN output.
    public const string SeqScanOperator = "Seq Scan";
    public const string TableScanOperator = "Table Scan";
    public const string ClusteredIndexScanOperator = "Clustered Index Scan";
    public const string IndexScanOperator = "Index Scan";

    // Minimum share of statement cost for a scan to be worth an index.
    public const double MinRelativeCost = 0.10;

    // Relative-cost cut-offs that map a scan's cost share to a baseline confidence.
    public const double HighConfidenceRelativeCost = 0.60;
    public const double MediumConfidenceRelativeCost = 0.30;

    public const double PercentScale = 100.0;

    // Match fraction assumed when the plan carries no row counts.
    public const double NeutralMatchFraction = 0.5;

    // Impact blend: a base share of the scan's cost is always recovered by seeking,
    // and the remainder scales with how much the filter eliminates.
    public const double SeekBaseFactor = 0.4;
    public const double SeekEliminationWeight = 0.6;

    public const int ImpactDecimals = 1;
}
