using System.Text;
using System.Linq;
using System.Collections.Generic;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Provides rendering capabilities for execution plans and index recommendations into a self‑contained HTML report.
/// The report includes an inline CSS stylesheet and a table of recommendations where rows are coloured
/// according to the recommendation confidence (severity).
/// </summary>
public static class HtmlReportRenderer
{
    /// <summary>
    /// Renders an execution plan and its index recommendations as an HTML document.
    /// </summary>
    /// <param name="plan">The execution plan containing dialect and cost information.</param>
    /// <param name="recs">The list of index recommendations to include in the report.</param>
    /// <returns>A string containing a complete HTML document.</returns>
    public static string RenderHtml(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs)
    {
        var sb = new StringBuilder();

        // HTML header with inline CSS
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>SQL Index Advisor Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: Arial, Helvetica, sans-serif; margin: 20px; }");
        sb.AppendLine("    h1 { margin-bottom: 0.5rem; }");
        sb.AppendLine("    .summary { margin-bottom: 1.5rem; }");
        sb.AppendLine("    table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("    th { background-color: #f2f2f2; }");
        sb.AppendLine("    tr:nth-child(even) { background-color: #fafafa; }");
        sb.AppendLine("    .high { background-color: #c8e6c9; }   /* green */");
        sb.AppendLine("    .medium { background-color: #fff9c4; } /* yellow */");
        sb.AppendLine("    .low { background-color: #ffcdd2; }    /* red */");
        sb.AppendLine("    .unknown { background-color: #e0e0e0; }");
        sb.AppendLine("    .reasons { margin: 0; padding-left: 1rem; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>SQL Index Advisor Report</h1>");
        sb.AppendLine("<div class=\"summary\">");
        sb.AppendLine($"<p><strong>Dialect:</strong> {plan.Dialect}</p>");
        sb.AppendLine($"<p><strong>Statement cost:</strong> {plan.EstimatedTotalCost:0.###}</p>");
        sb.AppendLine($"<p><strong>Operators:</strong> {plan.Nodes.Count}</p>");
        sb.AppendLine("</div>");

        if (recs.Count == 0)
        {
            sb.AppendLine("<p>No index recommendations. The plan looks fine or lacks the detail needed to suggest one.</p>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        sb.AppendLine($"<h2>{recs.Count} recommendation(s)</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>#</th>");
        sb.AppendLine("<th>Confidence</th>");
        sb.AppendLine("<th>Impact %</th>");
        sb.AppendLine("<th>Create Statement</th>");
        sb.AppendLine("<th>Reasons</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        int i = 1;
        foreach (var r in recs)
        {
            // Confidence is an enum (non‑nullable), so we just call ToString().
            var confidenceStr = r.Confidence.ToString();
            var cssClass = ConfidenceToCssClass(confidenceStr);
            sb.AppendLine($"<tr class=\"{cssClass}\">");
            sb.AppendLine($"<td>{i}</td>");
            sb.AppendLine($"<td>{confidenceStr}</td>");
            sb.AppendLine($"<td>{r.EstimatedImpactPercent:0.#}%</td>");
            sb.AppendLine($"<td><code>{System.Net.WebUtility.HtmlEncode(r.ToCreateStatement(plan.Dialect))}</code></td>");
            sb.AppendLine("<td><ul class=\"reasons\">");
            foreach (var reason in r.Reasons)
            {
                sb.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(reason)}</li>");
            }
            sb.AppendLine("</ul></td>");
            sb.AppendLine("</tr>");
            i++;
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("<p>Impact figures are rough heuristics, not measured gains. Validate before applying.</p>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string ConfidenceToCssClass(string confidence)
    {
        return confidence.ToLowerInvariant() switch
        {
            "high" => "high",
            "medium" => "medium",
            "low" => "low",
            _ => "unknown"
        };
    }
}
