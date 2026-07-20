using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Core.Reporting;

const string Usage = """
sql-index-advisor - recommend missing indexes from a query execution plan

USAGE:
    sql-index-advisor <plan-file> [--format text|json|html|csv] [--min-impact <n>]
    sql-index-advisor --stdin [--format text|json|html|csv]

ARGUMENTS:
    <plan-file>        Path to a SQL Server showplan XML or Postgres EXPLAIN (FORMAT JSON) file.

OPTIONS:
    --stdin            Read the plan from standard input instead of a file.
    --format <fmt>     Output format: text (default), json, html, or csv.
    --min-impact <n>   Hide recommendations below this estimated impact percent.
    -h, --help         Show this help.
""";

try
{
    return Run(args);
}
catch (PlanParseException ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

static int Run(string[] args)
{
    if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
    {
        Console.WriteLine(Usage);
        return args.Length == 0 ? 1 : 0;
    }

    string? path = null;
    var useStdin = false;
    var format = "text";
    double minImpact = 0;

    for (var i = 0; i < args.Length; i++)
    {
        var a = args[i];
        switch (a)
        {
            case "--stdin":
                useStdin = true;
                break;
            case "--format":
                format = RequireValue(args, ref i, "--format").ToLowerInvariant();
                break;
            case "--min-impact":
                var raw = RequireValue(args, ref i, "--min-impact");
                if (!double.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out minImpact))
                    throw new ArgumentException($"--min-impact expects a number, got '{raw}'.");
                break;
            default:
                if (a.StartsWith('-'))
                    throw new ArgumentException($"unknown option '{a}'.");
                path = a;
                break;
        }
    }

    if (format != "text" && format != "json" && format != "html" && format != "csv")
        throw new ArgumentException($"--format must be 'text', 'json', 'html', or 'csv', got '{format}'.");

    string content;
    if (useStdin)
    {
        content = Console.In.ReadToEnd();
    }
    else if (path is not null)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"plan file not found: {path}");
        content = File.ReadAllText(path);
    }
    else
    {
        throw new ArgumentException("provide a plan file path or --stdin.");
    }

    if (string.IsNullOrWhiteSpace(content))
        throw new PlanParseException("plan content is empty.");

    var plan = new PlanParserFactory().Parse(content);
    var recs = new RecommendationEngine().Analyze(plan);

    if (minImpact > 0)
        recs = recs.Where(r => r.EstimatedImpactPercent >= minImpact).ToList();

    string output = format switch
    {
        "json" => ReportRenderer.RenderJson(plan, recs),
        "html" => HtmlReportRenderer.RenderHtml(plan, recs),
        "csv"  => CsvReportRenderer.RenderCsv(plan, recs),
        _ => ReportRenderer.RenderText(plan, recs),
    };

    Console.WriteLine(output);
    return 0;
}

static string RequireValue(string[] args, ref int i, string name)
{
    if (i + 1 >= args.Length)
        throw new ArgumentException($"{name} requires a value.");
    return args[++i];
}
