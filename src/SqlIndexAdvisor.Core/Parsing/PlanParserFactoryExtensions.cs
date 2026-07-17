using System.Globalization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Provides extension methods for <see cref="PlanParserFactory"/> to simplify common parsing scenarios.
/// </summary>
public static class PlanParserFactoryExtensions
{
    /// <summary>
    /// Attempts to parse the execution plan content, returning a boolean indicating success and the parsed plan if successful.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <param name="content">The execution plan content to parse.</param>
    /// <param name="plan">When this method returns true, contains the parsed execution plan; otherwise, null.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="content"/> is null.</exception>
    public static bool TryParse(this PlanParserFactory factory, string content, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ExecutionPlan? plan)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            plan = factory.Parse(content);
            return true;
        }
        catch (PlanParseException)
        {
            plan = null;
            return false;
        }
    }

    /// <summary>
    /// Parses multiple execution plan contents and returns the successfully parsed plans with their source identifiers.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <param name="contents">A collection of plan content entries to parse.</param>
    /// <returns>An enumerable of tuples containing the source identifier and successfully parsed execution plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="contents"/> is null.</exception>
    public static IEnumerable<(string SourceId, ExecutionPlan Plan)> ParseMany(
        this PlanParserFactory factory,
        IEnumerable<(string SourceId, string Content)> contents)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(contents);

        foreach (var (sourceId, content) in contents)
        {
            if (factory.TryParse(content, out var plan))
            {
                yield return (sourceId, plan);
            }
        }
    }

    /// <summary>
    /// Determines whether the specified content can be parsed by any registered parser.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <param name="content">The execution plan content to check.</param>
    /// <returns>True if the content can be parsed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="content"/> is null.</exception>
    public static bool CanParse(this PlanParserFactory factory, string content)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(content);

        return factory.Resolve(content) is not null;
    }

    /// <summary>
    /// Gets the names of all registered parsers in the factory.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <returns>A read-only list containing the display names of all registered parsers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    public static IReadOnlyList<string> GetRegisteredParserNames(this PlanParserFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory.GetRegisteredParsers()
            .Select(p => p.GetType().Name)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all registered parsers in the factory.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <returns>A read-only list of all registered <see cref="IPlanParser"/> instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    public static IReadOnlyList<IPlanParser> GetRegisteredParsers(this PlanParserFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory.GetRegisteredParserNames()
            .Select(name => factory.Resolve("<?>"))
            .Where(p => p is not null)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Parses execution plan content with a custom parser selection strategy.
    /// </summary>
    /// <param name="factory">The plan parser factory instance.</param>
    /// <param name="content">The execution plan content to parse.</param>
    /// <param name="parserSelector">A function that selects a parser from the available parsers.</param>
    /// <returns>The parsed execution plan.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/>, <paramref name="content"/>, or <paramref name="parserSelector"/> is null.
    /// </exception>
    /// <exception cref="PlanParseException">Thrown when no parser is selected or parsing fails.</exception>
    public static ExecutionPlan ParseWith(
        this PlanParserFactory factory,
        string content,
        Func<IReadOnlyList<IPlanParser>, IPlanParser?> parserSelector)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(parserSelector);

        var parser = parserSelector(factory.GetRegisteredParsers());
        return parser is null
            ? throw new PlanParseException("No parser was selected by the provided selector function.")
            : parser.Parse(content);
    }
}

// Internal interface to expose the parsers collection for extension methods
// This allows the extension methods to access the internal parsers without reflection
internal interface IPlanParserFactoryInternal
{
    IReadOnlyList<IPlanParser> GetParsers();
}