using System;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses raw execution plan content into an <see cref="ExecutionPlan"/>.
/// Implementations typically handle a single vendor format (e.g. SQL Server showplan XML or PostgreSQL JSON).
/// </summary>
public interface IPlanParser
{
    /// <summary>True if this parser thinks it can handle the given raw content.</summary>
    bool CanParse(string content);

    /// <summary>
    /// Parses the supplied raw content into an <see cref="ExecutionPlan"/>.
    /// An optional <see cref="CancellationToken"/> can be supplied to abort the operation.
    /// </summary>
    ExecutionPlan Parse(string content, CancellationToken cancellationToken = default);
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
