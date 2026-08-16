using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Interface exposing the public test methods of <see cref="SqlServerParserTests"/>.
    /// </summary>
    public interface ISqlServerParserTests
    {
        void DetectsFormat();
        void ParsesDialectAndCost();
        void ExtractsEngineMissingIndex();
        void ExtractsScanPredicateColumns();
    }
}
