using System;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

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

public sealed class PlanParseException : Exception
{
    public PlanParseException(string message, Exception? inner = null) : base(message, inner) { }
}
