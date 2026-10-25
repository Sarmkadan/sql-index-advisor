namespace SqlIndexAdvisor.Core.ArgsParsing;

internal static class ArgsParserConstants
{
    public const string StdinFlag = "--stdin";
    public const string PlanFlag = "--plan";
    public const string FormatFlag = "--format";
    public const string MinImpactFlag = "--min-impact";
    public const string FailOnFindingsFlag = "--fail-on-findings";
    public const string FailOnFlag = "--fail-on";
    public const string SqlServerFlag = "--sqlserver";
    public const string PostgresFlag = "--postgres";
    public const string VersionFlag = "--version";
    public const string HelpShortFlag = "-h";
    public const string HelpFlag = "--help";

    public const string StdinPath = "-";

    public const string TextFormat = "text";
    public const string JsonFormat = "json";
    public const string HtmlFormat = "html";
    public const string CsvFormat = "csv";

    public const string LowSeverity = "low";
    public const string MediumSeverity = "medium";
    public const string HighSeverity = "high";
}
