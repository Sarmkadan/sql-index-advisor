using System.Collections.Generic;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Defines rendering capabilities for execution plans and index recommendations.
/// Implementations can provide different output formats (e.g., text, JSON) and are
/// selected by the CLI via a factory based on the requested format.
/// </summary>
public interface IReportRenderer
{
    /// <summary>
    /// Renders an execution plan and its index recommendations as a human‑readable text report.
    /// </summary>
    /// <param name="plan">The execution plan containing dialect and cost information.</param>
    /// <param name="recs">The list of index recommendations to include in the report.</param>
    /// <returns>A formatted text report string.</returns>
    string RenderText(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs);

    /// <summary>
    /// Renders an execution plan and its index recommendations as a JSON string.
    /// </summary>
    /// <param name="plan">The execution plan containing dialect and cost information.</param>
    /// <param name="recs">The list of index recommendations to include in the report.</param>
    /// <param name="schemaVersion">The schema version to include in the output.</param>
    /// <returns>A JSON‑formatted report string.</returns>
    string RenderJson(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs, string schemaVersion = "1.0");
}
