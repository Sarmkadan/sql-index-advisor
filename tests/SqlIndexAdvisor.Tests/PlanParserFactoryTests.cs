using System;
using Xunit;
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests
{
    public class PlanParserFactoryTests
    {
        private readonly PlanParserFactory _factory = new PlanParserFactory();

        [Fact]
        public void TryParse_XmlContent_ReturnsSqlServerXmlPlanParser()
        {
            // Minimal SQL Server XML plan that contains the ShowPlanXML root element.
            var xml = @"<ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan""><Batch></Batch></ShowPlanXML>";

            bool result = _factory.TryParse(xml, out var parser);

            Assert.True(result);
            Assert.NotNull(parser);
            Assert.IsType<SqlServerXmlPlanParser>(parser);
        }

        [Fact]
        public void TryParse_JsonContent_ReturnsPostgresJsonPlanParser()
        {
            // Minimal PostgreSQL JSON plan – the parser only needs to detect the JSON format.
            var json = @"{""Plan"":{}}";

            bool result = _factory.TryParse(json, out var parser);

            Assert.True(result);
            Assert.NotNull(parser);
            Assert.IsType<PostgresJsonPlanParser>(parser);
        }

        [Fact]
        public void TryParse_WhitespacePrefixedContent_DetectsCorrectly()
        {
            // Leading whitespace should not affect detection.
            var xml = @"   <ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan""><Batch></Batch></ShowPlanXML>";

            bool result = _factory.TryParse(xml, out var parser);

            Assert.True(result);
            Assert.NotNull(parser);
            Assert.IsType<SqlServerXmlPlanParser>(parser);
        }

        [Fact]
        public void Parse_UnrecognizedContent_ThrowsPlanParseException()
        {
            // Content that does not match any known parser format.
            var bad = "This is not a plan";

            Assert.Throws<PlanParseException>(() => _factory.Parse(bad));
        }
    }
}
