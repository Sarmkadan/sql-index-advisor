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
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(recs);

        var sb = new StringBuilder();

        sb.AppendLine(MarkdownReportRendererConstants.ReportTitle);
        sb.AppendLine();
        sb.AppendLine($"{MarkdownReportRendererConstants.DialectLabel}{EscapeCell(plan.Dialect.ToString())}");
        sb.AppendLine($"{MarkdownReportRendererConstants.EstimatedTotalCostLabel}{plan.EstimatedTotalCost.ToString("0.###", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine(MarkdownReportRendererConstants.RecommendationsHeading);
        sb.AppendLine();
        sb.AppendLine(MarkdownReportRendererConstants.TableHeader);
        sb.AppendLine(MarkdownReportRendererConstants.TableAlignmentRow);

        foreach (var recommendation in recs)
        {
            var keyColumns = string.Join(MarkdownReportRendererConstants.ColumnSeparator, recommendation.KeyColumns);
            var includeColumns = string.Join(MarkdownReportRendererConstants.ColumnSeparator, recommendation.IncludeColumns);
            var reasons = string.Join(MarkdownReportRendererConstants.ReasonSeparator, recommendation.Reasons);
            var impact = recommendation.EstimatedImpactPercent.ToString("0.#", CultureInfo.InvariantCulture);

            sb.AppendLine($"{MarkdownReportRendererConstants.TableRowStart}{EscapeCell(recommendation.Table)}{MarkdownReportRendererConstants.TableCellSeparator}{EscapeCell(keyColumns)}{MarkdownReportRendererConstants.TableCellSeparator}{EscapeCell(includeColumns)}{MarkdownReportRendererConstants.TableCellSeparator}{EscapeCell(recommendation.Confidence.ToString())}{MarkdownReportRendererConstants.TableCellSeparator}{impact}{MarkdownReportRendererConstants.TableCellSeparator}{EscapeCell(reasons)}{MarkdownReportRendererConstants.TableRowEnd}");
        }

        foreach (var recommendation in recs)
        {
            sb.AppendLine();
            sb.AppendLine($"{MarkdownReportRendererConstants.TableHeadingPrefix}{EscapeCell(recommendation.Table)}");
            sb.AppendLine();
            sb.AppendLine(MarkdownReportRendererConstants.SqlCodeFenceOpen);
            sb.AppendLine(DdlRenderer.RenderCreateIndex(recommendation, plan.Dialect));
            sb.AppendLine(MarkdownReportRendererConstants.CodeFenceClose);
        }

        return sb.ToString();
    }

    private static string EscapeCell(string value) => value.Replace(MarkdownReportRendererConstants.TableCellPipe, MarkdownReportRendererConstants.EscapedTableCellPipe);
}
