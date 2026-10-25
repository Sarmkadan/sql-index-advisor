namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Shared literals used by <see cref="ArgsParserTests"/>.
/// </summary>
internal static class ArgsParserTestsConstants
{
    /// <summary>
    /// Tool name expected in help output.
    /// </summary>
    public const string ToolName = "sql-index-advisor";

    /// <summary>
    /// Command-line flags.
    /// </summary>
    public const string HelpFlag = "--help";
    public const string HelpShortFlag = "-h";
    public const string StdinFlag = "--stdin";
    public const string FormatFlag = "--format";
    public const string MinImpactFlag = "--min-impact";
    public const string FailOnFlag = "--fail-on";
    public const string UnknownOptionFlag = "--unknown";
    public const string UnknownShortOptionFlag = "-x";

    /// <summary>
    /// Output format names.
    /// </summary>
    public const string TextFormat = "text";
    public const string JsonFormat = "json";
    public const string HtmlFormat = "html";
    public const string CsvFormat = "csv";
    public const string InvalidFormat = "invalid";
    public const string UpperCaseJsonFormat = "JSON";

    /// <summary>
    /// Severity values for <c>--fail-on</c>.
    /// </summary>
    public const string MixedCaseMediumSeverity = "Medium";
    public const string UnknownSeverity = "critical";

    /// <summary>
    /// Input paths and file names.
    /// </summary>
    public const string DashPath = "-";
    public const string TestFileName = "test.xml";
    public const string DotSlashTestFilePath = "./test.xml";
    public const string ParentTraversalPath = "../test.xml";
    public const string NestedParentTraversalPath = "../../test.xml";
    public const string ParentInMiddlePath = "subdir\\..\\test.xml";
    public const string AbsoluteOutsidePath = "/etc/passwd";
    public const string BackslashUncPath = "\\\\server\\share\\file.txt";
    public const string ForwardSlashUncPath = "//server/share/file.txt";
    public const string NonExistentFileName = "non-existent-file.txt";
    public const string Utf16LeSqlPlanSamplePath = "samples/sqlserver_orders_scan_utf16le.sqlplan";

    /// <summary>
    /// Content markers expected in the UTF-16 LE sample plan.
    /// </summary>
    public const string ShowPlanXmlMarker = "ShowPlanXML";
    public const string MissingIndexesMarker = "MissingIndexes";

    /// <summary>
    /// Text written to temporary files and stdin in encoding tests.
    /// </summary>
    public const string TestContent = "test content";

    /// <summary>
    /// <c>--min-impact</c> values as passed on the command line (a percentage in the 0..100 range).
    /// </summary>
    public const string NonNumericMinImpact = "not-a-number";
    public const string FilePathMinImpactArg = "25";
    public const string ValidMinImpactArg = "50.5";
    public const string CombinedMinImpactArg = "10.5";
    public const string StdinAllOptionsMinImpactArg = "75";

    /// <summary>
    /// Expected parsed <c>--min-impact</c> values.
    /// </summary>
    public const double DefaultMinImpact = 0;
    public const double FilePathMinImpact = 25;
    public const double ValidMinImpact = 50.5;
    public const double CombinedMinImpact = 10.5;
    public const double StdinAllOptionsMinImpact = 75;

    /// <summary>
    /// Expected error message fragments.
    /// </summary>
    public const string UnknownOptionMessage = "unknown option";
    public const string MissingFormatValueMessage = "--format requires a value";
    public const string InvalidFormatMessage = "--format must be";
    public const string MissingMinImpactValueMessage = "--min-impact requires a value";
    public const string InvalidMinImpactMessage = "--min-impact expects a number";
    public const string MinImpactRangeMessage = "--min-impact must be between 0 and 100";
    public const string StdinAndFilePathMessage = "cannot specify both --stdin and a file path";
    public const string PathTraversalMessage = "Path traversal sequences (..) are not allowed";
    public const string OutsideCurrentDirectoryMessage = "Paths outside the current directory are not allowed";
    public const string UncPathMessage = "UNC paths are not allowed";
    public const string FailOnSeverityMessage = "low, medium or high";
}
