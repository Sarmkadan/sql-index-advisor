using System.Text;
using System.Text.Json;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Provides rendering capabilities for execution plans and index recommendations into various output formats.
/// </summary>
public static class ReportRenderer
{
    /// <summary>
/// JSON serialization options configured for human-readable output with proper indentation.
/// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    	/// <summary>
	/// Renders an execution plan and its index recommendations as a human-readable text report.
	/// </summary>
	/// <param name="plan">The execution plan containing dialect and cost information.</param>
	/// <param name="recs">The list of index recommendations to include in the report.</param>
	/// <returns>A formatted text report string.</returns>
	public static string RenderText(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Dialect      : {plan.Dialect}");
        sb.AppendLine($"Statement cost: {plan.EstimatedTotalCost:0.###}");
        sb.AppendLine($"Operators     : {plan.Nodes.Count}");
        sb.AppendLine();

        if (recs.Count == 0)
        {
            sb.AppendLine("No index recommendations. The plan looks fine or lacks the detail needed to suggest one.");
            return sb.ToString();
        }

        sb.AppendLine($"{recs.Count} recommendation(s):");
        sb.AppendLine();
        var i = 1;
        foreach (var r in recs)
        {
            sb.AppendLine($"[{i}] {r.Confidence} confidence  |  ~{r.EstimatedImpactPercent:0.#}% estimated impact");
            sb.AppendLine($"    {r.ToCreateStatement(plan.Dialect)}");
            foreach (var reason in r.Reasons)
                sb.AppendLine($"      - {reason}");
            sb.AppendLine();
            i++;
        }

        sb.AppendLine("Impact figures are rough heuristics, not measured gains. Validate before applying.");
        return sb.ToString();
    }

    	/// <summary>
	/// Renders an execution plan and its index recommendations as a JSON string.
	/// </summary>
	/// <param name="plan">The execution plan containing dialect and cost information.</param>
	/// <param name="recs">The list of index recommendations to include in the report.</param>
	/// <returns>A JSON-formatted report string.</returns>
	public static string RenderJson(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs)
    {
        var payload = new
        {
            dialect = plan.Dialect.ToString(),
            estimatedTotalCost = plan.EstimatedTotalCost,
            operatorCount = plan.Nodes.Count,
            recommendations = recs.Select(r => new
            {
                table = r.Table,
                keyColumns = r.KeyColumns,
                includeColumns = r.IncludeColumns,
                estimatedImpactPercent = r.EstimatedImpactPercent,
                confidence = r.Confidence.ToString(),
                createStatement = r.ToCreateStatement(plan.Dialect),
                reasons = r.Reasons
            })
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }
}
