using SqlIndexAdvisor.Core.ArgsParsing;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
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
    const string usage = "Usage: sql-index-advisor <plan-file>|--stdin [--format text|json|html|csv|markdown] [options]";

    var parseResult = ArgsParser.Parse(args);

    if (parseResult.ShouldShowHelp)
    {
        Console.WriteLine(parseResult.HelpMessage ?? usage);
        var helpWasRequested = args.Any(arg => arg is "-h" or "--help" or "--version");
        return helpWasRequested ? 0 : 1;
    }

    if (parseResult.Format is not ("text" or "json" or "html" or "csv" or "markdown"))
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

    var plans = new PlanParserFactory().Parse(content);
    var engine = new RecommendationEngine();
    var reports = plans
        .Select((plan, index) => new StatementReport(index + 1, plan, FilterByImpact(engine.Analyze(plan), parseResult.MinImpact)))
        .ToList();

    Console.WriteLine(StatementReportRenderer.Render(reports, parseResult.Format, parseResult.SchemaVersion));

    // Exit code logic:
    // 0 = success, no findings (or findings ignored with --fail-on-findings false)
    // 1 = usage/IO error, or findings present and --fail-on-findings is true
    // 2 = parse error (already handled in catch block)
    var findingCount = reports.Sum(r => r.Recommendations.Count);
    return parseResult.FailOnFindings && findingCount > 0 ? 1 : 0;
}

static IReadOnlyList<IndexRecommendation> FilterByImpact(IReadOnlyList<IndexRecommendation> recs, double minImpact) =>
    minImpact > 0
        ? recs.Where(r => r.EstimatedImpactPercent >= minImpact).ToList()
        : recs;
