namespace SqlIndexAdvisor.Core.Reporting;

internal static class MarkdownReportRendererConstants
{
    public const string ReportTitle = "# SQL Index Advisor Report";
    public const string DialectLabel = "- **Dialect:** ";
    public const string EstimatedTotalCostLabel = "- **Estimated total cost:** ";
    public const string RecommendationsHeading = "## Recommendations";
    public const string TableHeader = "| Table | Key columns | Includes | Confidence | Estimated impact % | Reason |";
    public const string TableAlignmentRow = "| --- | --- | --- | --- | ---: | --- |";
    public const string TableRowStart = "| ";
    public const string TableRowEnd = " |";
    public const string TableCellSeparator = " | ";
    public const string TableCellPipe = "|";
    public const string EscapedTableCellPipe = "\\|";
    public const string ColumnSeparator = ", ";
    public const string ReasonSeparator = "; ";
    public const string TableHeadingPrefix = "### ";
    public const string SqlCodeFenceOpen = "```sql";
    public const string CodeFenceClose = "```";
}
