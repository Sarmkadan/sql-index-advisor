namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Holds constant values used by <see cref="ExecutionPlanSummaryExtensions"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class ExecutionPlanSummaryExtensionsConstants
{
    /// <summary>
    /// Format for <see cref="ExecutionPlanSummaryExtensions.GetSummary"/>: dialect, node count, scan count, estimated total cost.
    /// </summary>
    public const string SummaryFormat = "{0} plan: {1} nodes, {2} scans, estimated cost {3:F2}";
}
