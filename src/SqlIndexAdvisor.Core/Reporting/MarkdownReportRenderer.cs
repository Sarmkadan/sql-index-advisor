using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Renders execution plans and index recommendations as GitHub-flavored Markdown.
/// </summary>
public static class MarkdownReportRenderer
{
    /// <summary>
    /// Renders the supplied plan and recommendations as Markdown.
    /// </summary>
    /// <param name="plan">The execution plan containing dialect and cost information.</param>
    /// <param name="recs">The list of index recommendations.</param>
    /// <returns>A Markdown report.</returns>
    public static string RenderMarkdown(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# SQL Index Advisor Report");
        sb.AppendLine();
        sb.AppendLine($"- **Dialect:** {EscapeCell(plan.Dialect.ToString())}");
        sb.AppendLine($"- **Estimated total cost:** {plan.EstimatedTotalCost.ToString("0.###", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        sb.AppendLine();
        sb.AppendLine("| Table | Key columns | Includes | Confidence | Estimated impact % | Reason |");
        sb.AppendLine("| --- | --- | --- | --- | ---: | --- |");

        foreach (var recommendation in recs)
        {
            var keyColumns = string.Join(", ", recommendation.KeyColumns);
            var includeColumns = string.Join(", ", recommendation.IncludeColumns);
            var reasons = string.Join("; ", recommendation.Reasons);
            var impact = recommendation.EstimatedImpactPercent.ToString("0.#", CultureInfo.InvariantCulture);

            sb.AppendLine($"| {EscapeCell(recommendation.Table)} | {EscapeCell(keyColumns)} | {EscapeCell(includeColumns)} | {EscapeCell(recommendation.Confidence.ToString())} | {impact} | {EscapeCell(reasons)} |");
        }

        foreach (var recommendation in recs)
        {
            sb.AppendLine();
            sb.AppendLine($"### {EscapeCell(recommendation.Table)}");
            sb.AppendLine();
            sb.AppendLine("```sql");
            sb.AppendLine(DdlRenderer.RenderCreateIndex(recommendation, plan.Dialect));
            sb.AppendLine("```");
        }

        return sb.ToString();
    }

    private static string EscapeCell(string value) => value.Replace("|", "\\|");
}
