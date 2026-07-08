using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

public interface IPlanParser
{
    /// <summary>True if this parser thinks it can handle the given raw content.</summary>
    bool CanParse(string content);

    ExecutionPlan Parse(string content);
}

public sealed class PlanParseException : Exception
{
    public PlanParseException(string message, Exception? inner = null) : base(message, inner) { }
}
