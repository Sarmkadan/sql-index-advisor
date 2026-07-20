using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// Renders index recommendations as an RFC‑4180 compliant CSV document.
/// The columns are:
///   Confidence,ImpactPercent,CreateStatement,Reasons
/// </summary>
public static class CsvReportRenderer
{
    /// <summary>
    /// Renders the supplied plan and recommendations as CSV.
    /// </summary>
    /// <param name="plan">The execution plan (used for dialect when generating CREATE statements).</param>
    /// <param name="recs">The list of recommendations.</param>
    /// <returns>A CSV string.</returns>
    public static string RenderCsv(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Confidence,ImpactPercent,CreateStatement,Reasons");

        foreach (var r in recs)
        {
            var confidence = r.Confidence.ToString();
            var impact = r.EstimatedImpactPercent.ToString("0.#", CultureInfo.InvariantCulture);
            var createStmt = r.ToCreateStatement(plan.Dialect);
            var reasons = string.Join("; ", r.Reasons);

            sb.AppendLine(
                $"{Quote(confidence)},{Quote(impact)},{Quote(createStmt)},{Quote(reasons)}");
        }

        return sb.ToString();
    }

    // RFC‑4180 quoting: fields containing double quotes, commas, CR or LF must be quoted.
    // Inside quoted fields, double quotes are escaped by doubling them.
    private static string Quote(string field)
    {
        if (field == null)
            return string.Empty;

        bool mustQuote = field.Contains('"') ||
                         field.Contains(',') ||
                         field.Contains('\r') ||
                         field.Contains('\n');

        if (!mustQuote)
            return field;

        var escaped = field.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
