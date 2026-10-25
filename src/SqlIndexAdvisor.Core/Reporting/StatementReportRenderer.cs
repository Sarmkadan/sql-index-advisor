using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Renders the analysis of a plan input that may hold several statements. A single statement is rendered
/// exactly as the format's own renderer would render it. Several statements are grouped by statement, in
/// input order: text and Markdown get one titled section each, JSON becomes an array with one report per
/// statement, CSV keeps a single header followed by the rows of every statement, and HTML gets one section per statement.
/// </summary>
public static class StatementReportRenderer
{
    private const int MaxSummaryLength = 80;
    private const string EllipsisSuffix = "...";

    /// <summary>
    /// Renders the per-statement reports in the requested output format.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order. Must contain at least one report.</param>
    /// <param name="format">The output format: <c>text</c>, <c>json</c>, <c>html</c>, <c>csv</c> or <c>markdown</c>. Any other value renders text.</param>
    /// <param name="schemaVersion">The schema version written into JSON output.</param>
    /// <returns>The rendered report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports, format or schemaVersion is null.</exception>
    /// <exception cref="ArgumentException">Thrown when reports is empty or format is empty.</exception>
    public static string Render(IReadOnlyList<StatementReport> reports, string format, string schemaVersion = "1.0")
    {
        ArgumentNullException.ThrowIfNull(reports);
        ArgumentException.ThrowIfNullOrEmpty(format);
        ArgumentNullException.ThrowIfNull(schemaVersion);
        if (reports.Count == 0)
            throw new ArgumentException("At least one statement report is required.", nameof(reports));

        return reports.Count == 1
            ? RenderSingle(reports[0], format, schemaVersion)
            : RenderGrouped(reports, format, schemaVersion);
    }

    /// <summary>
    /// Renders a single statement with the format's own renderer.
    /// </summary>
    /// <param name="report">The report of the only statement.</param>
    /// <param name="format">The output format.</param>
    /// <param name="schemaVersion">The schema version written into JSON output.</param>
    /// <returns>The rendered report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    private static string RenderSingle(StatementReport report, string format, string schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(report);

        return format switch
        {
            "json" => ReportRenderer.RenderJson(report.Plan, report.Recommendations, schemaVersion),
            "html" => HtmlReportRenderer.RenderHtml(new[] { report }),
            "csv" => CsvReportRenderer.RenderCsv(report.Plan, report.Recommendations),
            "markdown" => MarkdownReportRenderer.RenderMarkdown(report.Plan, report.Recommendations),
            _ => ReportRenderer.RenderText(report.Plan, report.Recommendations),
        };
    }

    /// <summary>
    /// Renders several statements, grouping their output by statement in the requested format.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order. Contains at least two reports.</param>
    /// <param name="format">The output format.</param>
    /// <param name="schemaVersion">The schema version written into JSON output.</param>
    /// <returns>The grouped report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports is null.</exception>
    private static string RenderGrouped(IReadOnlyList<StatementReport> reports, string format, string schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(reports);

        return format switch
        {
            "json" => RenderJsonArray(reports, schemaVersion),
            "html" => HtmlReportRenderer.RenderHtml(reports),
            "csv" => RenderCsvRows(reports),
            "markdown" => RenderMarkdownSections(reports),
            _ => RenderTextSections(reports),
        };
    }

    /// <summary>
    /// Renders one JSON report per statement as a JSON array, in statement order.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order.</param>
    /// <param name="schemaVersion">The schema version written into each report.</param>
    /// <returns>A JSON array with one report object per statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports or schemaVersion is null.</exception>
    private static string RenderJsonArray(IReadOnlyList<StatementReport> reports, string schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(reports);
        ArgumentNullException.ThrowIfNull(schemaVersion);

        var reportsJson = reports.Select(r => ReportRenderer.RenderJson(r.Plan, r.Recommendations, schemaVersion));
        return $"[{Environment.NewLine}{string.Join($",{Environment.NewLine}", reportsJson)}{Environment.NewLine}]";
    }

    /// <summary>
    /// Renders the CSV rows of every statement under one header row, in statement order.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order.</param>
    /// <returns>A CSV document with a single header and the rows of every statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports is null.</exception>
    private static string RenderCsvRows(IReadOnlyList<StatementReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

        var sb = new StringBuilder();
        for (var i = 0; i < reports.Count; i++)
        {
            var csv = CsvReportRenderer.RenderCsv(reports[i].Plan, reports[i].Recommendations);
            // The first line is the header, which every statement repeats; keep it only once.
            sb.Append(i == 0 ? csv : csv[(csv.IndexOf('\n') + 1)..]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Renders one titled text section per statement.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order.</param>
    /// <returns>The text report with one section per statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports is null.</exception>
    private static string RenderTextSections(IReadOnlyList<StatementReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

        var sb = new StringBuilder();
        foreach (var report in reports)
        {
            sb.AppendLine($"=== {SectionTitle(report, reports.Count)} ===");
            sb.Append(ReportRenderer.RenderText(report.Plan, report.Recommendations));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Renders one Markdown section per statement, each headed by the statement's position and text.
    /// </summary>
    /// <param name="reports">The per-statement reports, in input order.</param>
    /// <returns>The Markdown report with one section per statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reports is null.</exception>
    private static string RenderMarkdownSections(IReadOnlyList<StatementReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

        var sb = new StringBuilder();
        foreach (var report in reports)
        {
            sb.AppendLine($"## {SectionTitle(report, reports.Count)}");
            sb.AppendLine();
            sb.Append(MarkdownReportRenderer.RenderMarkdown(report.Plan, report.Recommendations));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Builds the heading of a statement section from its position and a short summary of its text.
    /// </summary>
    /// <param name="report">The statement report to title.</param>
    /// <param name="total">The number of statements in the input.</param>
    /// <returns>A title such as <c>Statement 2 of 3: SELECT ...</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    private static string SectionTitle(StatementReport report, int total)
    {
        ArgumentNullException.ThrowIfNull(report);

        var title = $"Statement {report.Index} of {total}";
        var summary = Summarize(report.Plan.StatementText);
        return summary.Length == 0 ? title : $"{title}: {summary}";
    }

    /// <summary>
    /// Collapses a statement's text onto one line and shortens it to the summary length.
    /// </summary>
    /// <param name="statementText">The raw statement text, possibly empty or multi-line.</param>
    /// <returns>The single-line summary, or an empty string when there is no text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when statementText is null.</exception>
    private static string Summarize(string statementText)
    {
        ArgumentNullException.ThrowIfNull(statementText);

        var singleLine = string.Join(' ', statementText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return singleLine.Length <= MaxSummaryLength
            ? singleLine
            : singleLine[..(MaxSummaryLength - EllipsisSuffix.Length)] + EllipsisSuffix;
    }
}
