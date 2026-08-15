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
        /// Parses the given plan content into an <see cref="ExecutionPlan"/>.
        /// </summary>
        /// <param name="content">The raw plan content.</param>
        /// <returns>The parsed <see cref="ExecutionPlan"/>.</returns>
        ExecutionPlan Parse(string content);
    }
}
