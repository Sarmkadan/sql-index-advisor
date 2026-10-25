using System.Globalization;
using System.Linq;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Extension methods for <see cref="ExecutionPlan"/> that produce read-only counts and human-readable summaries.
/// </summary>
public static class ExecutionPlanSummaryExtensions
{
    /// <summary>
    /// Gets the number of operators in the flattened execution plan.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>The number of nodes in <see cref="ExecutionPlan.Nodes"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static int GetNodeCount(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes.Count;
    }

    /// <summary>
    /// Gets a single-line summary of the execution plan: dialect, node count, scan count and estimated total cost.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>A human-readable summary line for the plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static string GetSummary(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var scanCount = plan.Nodes.Count(node => node.IsScan);

        return string.Format(
            CultureInfo.InvariantCulture,
            ExecutionPlanSummaryExtensionsConstants.SummaryFormat,
            plan.Dialect,
            plan.GetNodeCount(),
            scanCount,
            plan.EstimatedTotalCost);
    }
}
