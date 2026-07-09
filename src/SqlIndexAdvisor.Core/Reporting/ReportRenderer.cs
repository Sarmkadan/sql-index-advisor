using System.Text;
using System.Text.Json;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

public static class ReportRenderer
{
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
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}
