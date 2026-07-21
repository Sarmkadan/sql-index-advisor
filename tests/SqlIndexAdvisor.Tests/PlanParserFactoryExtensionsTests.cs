using System;
using System.Collections.Generic;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public class PlanParserFactoryExtensionsTests
    {
        private readonly PlanParserFactory _factory = new PlanParserFactory();

        #region TryParse

        [Fact]
        public void TryParse_ValidXml_ReturnsTrueAndPlan()
        {
            var xml = @"<ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan""><Batch></Batch></ShowPlanXML>";

            var result = _factory.TryParse(xml, out var plan);

            Assert.True(result);
            Assert.NotNull(plan);
            Assert.IsType<ExecutionPlan>(plan);
        }

        [Fact]
        public void TryParse_ValidJson_ReturnsTrueAndPlan()
        {
            var json = @"{""Plan"":{}}";

            var result = _factory.TryParse(json, out var plan);

            Assert.True(result);
            Assert.NotNull(plan);
            Assert.IsType<ExecutionPlan>(plan);
        }

        [Fact]
        public void TryParse_InvalidContent_ReturnsFalseAndNullPlan()
        {
            var bad = "not a plan";

            var result = _factory.TryParse(bad, out var plan);

            Assert.False(result);
            Assert.Null(plan);
        }

        [Theory]
        [InlineData(null, "content")]
        [InlineData("content", null)]
        public void TryParse_NullArguments_ThrowsArgumentNullException(string factoryArg, string contentArg)
        {
            // Arrange
            PlanParserFactory? factory = factoryArg == null ? null : _factory;
            string? content = contentArg == null ? null : "dummy";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory!.TryParse(content!, out var _));
        }

        #endregion

        #region ParseMany

        [Fact]
        public void ParseMany_MixedContents_ReturnsOnlyParsable()
        {
            var inputs = new List<(string SourceId, string Content)>
            {
                ("xml1", @"<ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan""><Batch></Batch></ShowPlanXML>"),
                ("json1", @"{""Plan"":{}}"),
                ("bad", "not a plan")
            };

            var results = _factory.ParseMany(inputs).ToList();

            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.SourceId == "xml1" && r.Plan != null);
            Assert.Contains(results, r => r.SourceId == "json1" && r.Plan != null);
        }

        [Fact]
        public void ParseMany_EmptyCollection_ReturnsEmpty()
        {
            var results = _factory.ParseMany(Enumerable.Empty<(string, string)>()).ToList();
            Assert.Empty(results);
        }

        [Fact]
        public void ParseMany_NullFactory_ThrowsArgumentNullException()
        {
            PlanParserFactory? nullFactory = null;
            var inputs = new List<(string, string)> { ("x", "y") };
            Assert.Throws<ArgumentNullException>(() => nullFactory!.ParseMany(inputs));
        }

        [Fact]
        public void ParseMany_NullContents_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.ParseMany(null!));
        }

        #endregion

        #region CanParse

        [Fact]
        public void CanParse_RecognizedXml_ReturnsTrue()
        {
            var xml = @"<ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan""><Batch></Batch></ShowPlanXML>";
            Assert.True(_factory.CanParse(xml));
        }

        [Fact]
        public void CanParse_RecognizedJson_ReturnsTrue()
        {
            var json = @"{""Plan"":{}}";
            Assert.True(_factory.CanParse(json));
        }

        [Fact]
        public void CanParse_UnrecognizedContent_ReturnsFalse()
        {
            Assert.False(_factory.CanParse("random text"));
        }

        [Fact]
        public void CanParse_NullArguments_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.CanParse(null!));
        }

        #endregion

        #region GetRegisteredParserNames

        [Fact]
        public void GetRegisteredParserNames_ContainsExpectedParsers()
        {
            var names = _factory.GetRegisteredParserNames();

            Assert.Contains("SqlServerXmlPlanParser", names);
            Assert.Contains("PostgresJsonPlanParser", names);
        }

        #endregion

        #region GetRegisteredParsers

        [Fact]
        public void GetRegisteredParsers_ReturnsParsersWithExpectedTypes()
        {
            var parsers = _factory.GetRegisteredParsers();

            Assert.Contains(parsers, p => p.GetType().Name == "SqlServerXmlPlanParser");
            Assert.Contains(parsers, p => p.GetType().Name == "PostgresJsonPlanParser");
        }

        #endregion

        #region ParseWith

        [Fact]
        public void ParseWith_SelectorChoosesCorrectParser_ParsesSuccessfully()
        {
            var json = @"{""Plan"":{}}";

            ExecutionPlan plan = _factory.ParseWith(
                json,
                parsers => parsers.FirstOrDefault(p => p.GetType().Name == "PostgresJsonPlanParser"));

            Assert.NotNull(plan);
        }

        [Fact]
        public void ParseWith_SelectorReturnsNull_ThrowsPlanParseException()
        {
            var json = @"{""Plan"":{}}";

            Assert.Throws<PlanParseException>(() =>
                _factory.ParseWith(json, parsers => null));
        }

        [Fact]
        public void ParseWith_NullArguments_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((PlanParserFactory)null!).ParseWith("{}", parsers => parsers.First()));
            Assert.Throws<ArgumentNullException>(() => _factory.ParseWith(null!, parsers => parsers.First()));
            Assert.Throws<ArgumentNullException>(() => _factory.ParseWith("{}", null!));
        }

        #endregion
    }
}
