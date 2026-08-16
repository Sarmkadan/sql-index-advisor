using System;

namespace SqlIndexAdvisor.Tests
{
    public interface IPlanParserFactoryTests
    {
        void TryParse_XmlContent_ReturnsSqlServerXmlPlanParser();
        void TryParse_JsonContent_ReturnsPostgresJsonPlanParser();
        void TryParse_WhitespacePrefixedContent_DetectsCorrectly();
        void Parse_UnrecognizedContent_ThrowsPlanParseException();
    }
}
