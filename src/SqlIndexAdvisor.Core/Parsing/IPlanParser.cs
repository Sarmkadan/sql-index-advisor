using System;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses raw execution plan content into one <see cref="ExecutionPlan"/> per statement.
/// Implementations typically handle a single vendor format (e.g. SQL Server showplan XML or PostgreSQL JSON).
/// </summary>
public interface IPlanParser
{
    /// <summary>True if this parser thinks it can handle the given raw content.</summary>
    bool CanParse(string content);

    /// <summary>
    /// Parses the supplied raw content into one <see cref="ExecutionPlan"/> per statement, in the order
    /// the statements appear in the input. A single-statement input yields a list with one plan.
    /// An optional <see cref="CancellationToken"/> can be supplied to abort the operation.
    /// </summary>
    /// <param name="content">The raw plan content to parse.</param>
    /// <param name="cancellationToken">A token that can be used to cancel parsing.</param>
    /// <returns>A read-only list holding one execution plan per statement, never empty.</returns>
    /// <exception cref="PlanParseException">The content is malformed or holds no executable plan.</exception>
    IReadOnlyList<ExecutionPlan> Parse(string content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Thrown when execution plan content cannot be parsed or its format cannot be detected.
/// </summary>
public sealed class PlanParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanParseException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of this exception, if any.</param>
    public PlanParseException(string message, Exception? inner = null) : base(message, inner) { }
}
