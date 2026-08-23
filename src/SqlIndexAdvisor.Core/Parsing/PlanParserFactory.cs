using System;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Sniffs the content and hands back the right parser. Order matters only in
/// that each parser's CanParse is cheap and mutually exclusive in practice
/// (XML starts with '<', JSON with '[' or '{').
/// </summary>
public sealed class PlanParserFactory : IPlanParserFactory
{
    private readonly IReadOnlyList<IPlanParser> _parsers;

    /// <summary>
    /// Initializes a new instance with the built-in SQL Server XML and PostgreSQL JSON parsers.
    /// </summary>
    public PlanParserFactory()
        : this(new IPlanParser[] { new SqlServerXmlPlanParser(), new PostgresJsonPlanParser() })
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom set of parsers, tried in registration order.
    /// </summary>
    /// <param name="parsers">The parsers to try, in order, when resolving plan content.</param>
    public PlanParserFactory(IEnumerable<IPlanParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    /// <summary>
    /// Returns the first registered parser whose <see cref="IPlanParser.CanParse"/> accepts the supplied content.
    /// </summary>
    /// <param name="content">The raw plan content to sniff.</param>
    /// <returns>The first parser capable of handling the content.</returns>
    /// <exception cref="PlanParseException">
    /// Thrown when none of the registered parsers can handle the content.
    /// </exception>
    public IPlanParser Resolve(string content)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(content));
        if (parser is null)
        {
            var attemptedParsers = _parsers.Select(p => p.GetType().Name).ToList();
            throw new PlanParseException(
                $"Could not detect plan format. Content starts with '{content.TrimStart()[..Math.Min(20, content.TrimStart().Length)]}...'. " +
                "Expected SQL Server showplan XML (starting with '<' and containing 'ShowPlanXML' or 'StmtSimple') " +
                "or PostgreSQL JSON plan (starting with '[' or '{' and containing 'Plan' property). " +
                $"Tried parsers: {string.Join(", ", attemptedParsers)}.");
        }
        return parser;
    }

    /// <summary>
    /// Resolves the appropriate parser for the supplied content and parses it into an <see cref="ExecutionPlan"/>.
    /// </summary>
    /// <param name="content">The raw plan content to parse.</param>
    /// <returns>The parsed <see cref="ExecutionPlan"/>.</returns>
    /// <exception cref="PlanParseException">
    /// Thrown when no parser can handle the content or parsing fails.
    /// </exception>
    public ExecutionPlan Parse(string content) => Resolve(content).Parse(content);

    internal IReadOnlyList<IPlanParser> GetRegisteredParsersInternal() => _parsers;
}
