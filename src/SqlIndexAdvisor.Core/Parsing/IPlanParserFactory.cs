using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing
{
    /// <summary>
    /// Defines the contract for a factory that resolves and parses execution plans.
    /// </summary>
    public interface IPlanParserFactory
    {
        /// <summary>
        /// Resolves the appropriate <see cref="IPlanParser"/> for the given content.
        /// </summary>
        /// <param name="content">The raw plan content.</param>
        /// <returns>An <see cref="IPlanParser"/> capable of parsing the content.</returns>
        IPlanParser Resolve(string content);

        /// <summary>
        /// Parses the given plan content into one <see cref="ExecutionPlan"/> per statement.
        /// </summary>
        /// <param name="content">The raw plan content.</param>
        /// <returns>A read-only list with one parsed <see cref="ExecutionPlan"/> per statement.</returns>
        IReadOnlyList<ExecutionPlan> Parse(string content);
    }
}
