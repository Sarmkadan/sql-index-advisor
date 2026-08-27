using System;
using Xunit;
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="PlanParserFactory"/> class, covering XML and JSON plan detection and parsing.
    /// </summary>
    public class PlanParserFactoryTests : IPlanParserFactoryTests
    {
        /// <summary>
        /// Factory instance under test, initialized with the default set of registered plan parsers.
        /// </summary>
        private readonly PlanParserFactory _factory = new PlanParserFactory();

        /// <summary>
        /// Verifies that <c>TryParse</c> recognizes SQL Server XML plan content by detecting the ShowPlanXML root element
        /// and returns <c>true</c> together with a <see cref="SqlServerXmlPlanParser"/> instance.
        /// </summary>
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

        /// <summary>
        /// Verifies that <c>TryParse</c> recognizes PostgreSQL JSON plan content by detecting JSON format
        /// and returns <c>true</c> together with a <see cref="PostgresJsonPlanParser"/> instance.
        /// </summary>
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

        /// <summary>
        /// Verifies that <c>TryParse</c> correctly detects plan format when content has leading whitespace,
        /// ensuring whitespace does not interfere with format detection.
        /// </summary>
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

        /// <summary>
        /// Verifies that <c>Parse</c> throws a <see cref="PlanParseException"/> when given content
        /// that does not match any known plan format (neither SQL Server XML nor PostgreSQL JSON).
        /// </summary>
        [Fact]
        public void Parse_UnrecognizedContent_ThrowsPlanParseException()
        {
            // Content that does not match any known parser format.
            var bad = "This is not a plan";

            Assert.Throws<PlanParseException>(() => _factory.Parse(bad));
        }
    }
}
