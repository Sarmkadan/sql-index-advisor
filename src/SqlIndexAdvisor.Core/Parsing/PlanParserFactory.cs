using System.Text.Json.Serialization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Sniffs the content and hands back the right parser. Order matters only in
/// that each parser's CanParse is cheap and mutually exclusive in practice
/// (XML starts with '<', JSON with '[' or '{').
/// </summary>
[JsonSerializable(typeof(PlanParserFactory))]
public sealed class PlanParserFactory
{
    private readonly IReadOnlyList<IPlanParser> _parsers;

    public PlanParserFactory()
        : this(new IPlanParser[] { new SqlServerXmlPlanParser(), new PostgresJsonPlanParser() })
    {
    }

    public PlanParserFactory(IEnumerable<IPlanParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public IPlanParser Resolve(string content)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(content));
        if (parser is null)
            throw new PlanParseException(
                "Could not detect plan format. Expected SQL Server showplan XML or Postgres EXPLAIN (FORMAT JSON).");
        return parser;
    }

    public ExecutionPlan Parse(string content) => Resolve(content).Parse(content);

    internal IReadOnlyList<IPlanParser> GetRegisteredParsersInternal() => _parsers;
}