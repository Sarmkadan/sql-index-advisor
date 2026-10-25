using System;
using System.Collections.Generic;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Convenience extension methods for <see cref="IReportRenderer"/> that compose its public rendering members.
/// </summary>
public static class IReportRendererExtensions
{
    /// <summary>
    /// Renders the same plan and recommendations as both a text report and a JSON report.
    /// </summary>
    /// <param name="renderer">The renderer used to produce both reports.</param>
    /// <param name="plan">The execution plan containing dialect and cost information.</param>
    /// <param name="recs">The list of index recommendations to include in the reports.</param>
    /// <param name="schemaVersion">The schema version to include in the JSON output.</param>
    /// <returns>A tuple holding the text report (<c>Text</c>) and the JSON report (<c>Json</c>).</returns>
    /// <exception cref="ArgumentNullException">Thrown when renderer, plan, recs, or schemaVersion is null.</exception>
    public static (string Text, string Json) RenderTextAndJson(
        this IReportRenderer renderer,
        ExecutionPlan plan,
        IReadOnlyList<IndexRecommendation> recs,
        string schemaVersion = "1.0")
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(recs);
        ArgumentNullException.ThrowIfNull(schemaVersion);

        return (renderer.RenderText(plan, recs), renderer.RenderJson(plan, recs, schemaVersion));
    }
}
