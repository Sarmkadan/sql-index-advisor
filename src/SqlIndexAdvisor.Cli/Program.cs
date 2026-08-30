using SqlIndexAdvisor.Core.ArgsParsing;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Core.Reporting;

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
    const string usage = "Usage: sql-index-advisor <plan-file>|--stdin [--format text|json|html|csv] [options]";

    var parseResult = ArgsParser.Parse(args);

    if (parseResult.ShouldShowHelp)
    {
        Console.WriteLine(parseResult.HelpMessage ?? usage);
        var helpWasRequested = args.Any(arg => arg is "-h" or "--help" or "--version");
        return helpWasRequested ? 0 : 1;
    }

    if (parseResult.Format is not ("text" or "json" or "html" or "csv"))
    {
        Console.Error.WriteLine($"error: unknown format '{parseResult.Format}'");
        return 1;
    }

    string content;
    if (parseResult.UseStdin || parseResult.Path == "-")
    {
        content = ArgsParser.ReadFileWithEncoding("-");
    }
    else if (parseResult.Path is not null)
    {
        content = ArgsParser.ReadFileWithEncoding(parseResult.Path);
    }
    else
    {
        throw new ArgumentException("provide a plan file path or --stdin.");
    }

    if (string.IsNullOrWhiteSpace(content))
        throw new PlanParseException("plan content is empty.");

    var plan = new PlanParserFactory().Parse(content);
    var recs = new RecommendationEngine().Analyze(plan);

    if (parseResult.MinImpact > 0)
        recs = recs.Where(r => r.EstimatedImpactPercent >= parseResult.MinImpact).ToList();

    string output = parseResult.Format switch
    {
        "json" => ReportRenderer.RenderJson(plan, recs, parseResult.SchemaVersion),
        "html" => HtmlReportRenderer.RenderHtml(plan, recs),
        "csv" => CsvReportRenderer.RenderCsv(plan, recs),
        _ => ReportRenderer.RenderText(plan, recs),
    };

    Console.WriteLine(output);

    // Exit code logic:
    // 0 = success, no findings (or findings ignored with --fail-on-findings false)
    // 1 = usage/IO error, or findings present and --fail-on-findings is true
    // 2 = parse error (already handled in catch block)
    return parseResult.FailOnFindings && recs.Count > 0 ? 1 : 0;
}
