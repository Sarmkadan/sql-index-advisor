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
    var parseResult = ArgsParser.Parse(args);

    if (parseResult.ShouldShowHelp)
    {
        Console.WriteLine(parseResult.HelpMessage ?? ArgsParser.Parse(Array.Empty<string>()).HelpMessage);
        return parseResult.HelpMessage is null ? 1 : 0;
    }

    string content;
    if (parseResult.UseStdin || parseResult.Path == "-")
    {
        content = Console.In.ReadToEnd();
    }
    else if (parseResult.Path is not null)
    {
        if (!File.Exists(parseResult.Path))
            throw new FileNotFoundException($"plan file not found: {parseResult.Path}");
        content = File.ReadAllText(parseResult.Path);
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
        "json" => ReportRenderer.RenderJson(plan, recs),
        "html" => HtmlReportRenderer.RenderHtml(plan, recs),
        "csv" => CsvReportRenderer.RenderCsv(plan, recs),
        _ => ReportRenderer.RenderText(plan, recs),
    };

    Console.WriteLine(output);
    return 0;
}
