using System;
using System.Collections.Generic;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="PlanParserFactory"/> extension methods, covering TryParse,
    /// ParseMany, CanParse, GetRegisteredParserNames, GetRegisteredParsers and ParseWith.
    /// </summary>
    public class PlanParserFactoryExtensionsTests
    {
        /// <summary>
        /// Factory instance under test, initialized with the default set of registered plan parsers.
        /// </summary>
        private readonly PlanParserFactory _factory = new PlanParserFactory();

        #region TryParse

        /// <summary>
        /// Verifies that <c>TryParse</c> recognizes well-formed SQL Server XML plan content and
        /// returns <c>true</c> together with a non-null <see cref="ExecutionPlan"/>.
        /// </summary>
        [Fact]
        public void TryParse_ValidXml_ReturnsTrueAndPlan()
        {
            var xml = PlanParserFactoryExtensionsTestsConstants.ValidXml;

            var result = _factory.TryParse(xml, out var plan);

            Assert.True(result);
            Assert.NotNull(plan);
            Assert.IsType<ExecutionPlan>(plan);
        }

        /// <summary>
        /// Verifies that <c>TryParse</c> recognizes well-formed PostgreSQL JSON plan content and
        /// returns <c>true</c> together with a non-null <see cref="ExecutionPlan"/>.
        /// </summary>
        [Fact]
        public void TryParse_ValidJson_ReturnsTrueAndPlan()
        {
            var json = PlanParserFactoryExtensionsTestsConstants.ValidJson;

            var result = _factory.TryParse(json, out var plan);

            Assert.True(result);
            Assert.NotNull(plan);
            Assert.IsType<ExecutionPlan>(plan);
        }

        /// <summary>
        /// Verifies that <c>TryParse</c> reports failure for content matching no registered parser
        /// format, returning <c>false</c> and leaving the output plan null.
        /// </summary>
        [Fact]
        public void TryParse_InvalidContent_ReturnsFalseAndNullPlan()
        {
            var bad = PlanParserFactoryExtensionsTestsConstants.InvalidContent;

            var result = _factory.TryParse(bad, out var plan);

            Assert.False(result);
            Assert.Null(plan);
        }

        /// <summary>
        /// Verifies that <c>TryParse</c> throws an <see cref="ArgumentNullException"/> when either
        /// the factory instance or the plan content is null.
        /// </summary>
        /// <param name="factoryArg">InlineData marker; when null, the factory argument passed to <c>TryParse</c> is null.</param>
        /// <param name="contentArg">InlineData marker; when null, the content argument passed to <c>TryParse</c> is null.</param>
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

        /// <summary>
        /// Verifies that <c>ParseMany</c> skips unparsable entries and yields results only for the
        /// XML and JSON inputs that parse successfully.
        /// </summary>
        [Fact]
        public void ParseMany_MixedContents_ReturnsOnlyParsable()
        {
            var inputs = new List<(string SourceId, string Content)>
            {
                ("xml1", PlanParserFactoryExtensionsTestsConstants.ValidXml),
                ("json1", PlanParserFactoryExtensionsTestsConstants.ValidJson),
                ("bad", PlanParserFactoryExtensionsTestsConstants.InvalidContent)
            };

            var results = _factory.ParseMany(inputs).ToList();

            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.SourceId == "xml1" && r.Plan != null);
            Assert.Contains(results, r => r.SourceId == "json1" && r.Plan != null);
        }

        /// <summary>
        /// Verifies that <c>ParseMany</c> applied to an empty input collection produces no results.
        /// </summary>
        [Fact]
        public void ParseMany_EmptyCollection_ReturnsEmpty()
        {
            var results = _factory.ParseMany(Enumerable.Empty<(string, string)>()).ToList();
            Assert.Empty(results);
        }

        /// <summary>
        /// Verifies that invoking <c>ParseMany</c> on a null factory instance throws an
        /// <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void ParseMany_NullFactory_ThrowsArgumentNullException()
        {
            PlanParserFactory? nullFactory = null;
            var inputs = new List<(string, string)> { ("x", "y") };
            Assert.Throws<ArgumentNullException>(() => nullFactory!.ParseMany(inputs));
        }

        /// <summary>
        /// Verifies that passing a null collection of source contents to <c>ParseMany</c> throws
        /// an <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void ParseMany_NullContents_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.ParseMany(null!));
        }

        #endregion

        #region CanParse

        /// <summary>
        /// Verifies that <c>CanParse</c> detects valid SQL Server XML plan content as parsable.
        /// </summary>
        [Fact]
        public void CanParse_RecognizedXml_ReturnsTrue()
        {
            var xml = PlanParserFactoryExtensionsTestsConstants.ValidXml;
            Assert.True(_factory.CanParse(xml));
        }

        /// <summary>
        /// Verifies that <c>CanParse</c> detects valid PostgreSQL JSON plan content as parsable.
        /// </summary>
        [Fact]
        public void CanParse_RecognizedJson_ReturnsTrue()
        {
            var json = PlanParserFactoryExtensionsTestsConstants.ValidJson;
            Assert.True(_factory.CanParse(json));
        }

        /// <summary>
        /// Verifies that <c>CanParse</c> rejects content in no known plan format by returning <c>false</c>.
        /// </summary>
        [Fact]
        public void CanParse_UnrecognizedContent_ReturnsFalse()
        {
            Assert.False(_factory.CanParse(PlanParserFactoryExtensionsTestsConstants.InvalidContent));
        }

        /// <summary>
        /// Verifies that passing null content to <c>CanParse</c> throws an
        /// <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void CanParse_NullArguments_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.CanParse(null!));
        }

        #endregion

        #region GetRegisteredParserNames

        /// <summary>
        /// Verifies that the registered parser names include both the SQL Server XML plan parser
        /// and the PostgreSQL JSON plan parser.
        /// </summary>
        [Fact]
        public void GetRegisteredParserNames_ContainsExpectedParsers()
        {
            var names = _factory.GetRegisteredParserNames();

            Assert.Contains("SqlServerXmlPlanParser", names);
            Assert.Contains("PostgresJsonPlanParser", names);
        }

        #endregion

        #region GetRegisteredParsers

        /// <summary>
        /// Verifies that the registered parser instances include objects of the expected SQL Server
        /// XML and PostgreSQL JSON parser types.
        /// </summary>
        [Fact]
        public void GetRegisteredParsers_ReturnsParsersWithExpectedTypes()
        {
            var parsers = _factory.GetRegisteredParsers();

            Assert.Contains(parsers, p => p.GetType().Name == "SqlServerXmlPlanParser");
            Assert.Contains(parsers, p => p.GetType().Name == "PostgresJsonPlanParser");
        }

        #endregion

        #region ParseWith

        /// <summary>
        /// Verifies that <c>ParseWith</c> parses JSON plan content successfully when the selector
        /// picks the PostgreSQL JSON plan parser.
        /// </summary>
        [Fact]
        public void ParseWith_SelectorChoosesCorrectParser_ParsesSuccessfully()
        {
            var json = @"{""Plan"":{}}";

            ExecutionPlan plan = _factory.ParseWith(
                json,
                parsers => parsers.FirstOrDefault(p => p.GetType().Name == "PostgresJsonPlanParser"));

            Assert.NotNull(plan);
        }

        /// <summary>
        /// Verifies that <c>ParseWith</c> throws a <see cref="PlanParseException"/> when the
        /// selector does not return any parser.
        /// </summary>
        [Fact]
        public void ParseWith_SelectorReturnsNull_ThrowsPlanParseException()
        {
            var json = @"{""Plan"":{}}";

            Assert.Throws<PlanParseException>(() =>
                _factory.ParseWith(json, parsers => null));
        }

        /// <summary>
        /// Verifies that <c>ParseWith</c> throws an <see cref="ArgumentNullException"/> when the
        /// factory instance, the plan content, or the parser selector is null.
        /// </summary>
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
