using System;
using System.Linq;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Tests for the <see cref="PlanParserFactoryJsonExtensions"/> class.
    /// </summary>
    public class PlanParserFactoryJsonExtensionsTests : IPlanParserFactoryJsonExtensionsTests
    {
        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.ToJson(PlanParserFactory)"/> throws an <see cref="ArgumentNullException"/> when the factory is null.
        /// </summary>
        [Fact]
        public void ToJson_NullFactory_ThrowsArgumentNullException()
        {
            PlanParserFactory? factory = null;
            Assert.Throws<ArgumentNullException>(() => factory!.ToJson());
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.ToJson(PlanParserFactory)"/> returns non-indented JSON by default.
        /// </summary>
        [Fact]
        public void ToJson_Default_IsNonIndentedJson()
        {
            var factory = new PlanParserFactory();
            string json = factory.ToJson(); // default indented = false

            Assert.False(string.IsNullOrWhiteSpace(json));
            // Default (non‑indented) JSON should not contain line breaks.
            Assert.DoesNotContain("\n", json);
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.ToJson(PlanParserFactory)"/> returns indented JSON when the indented parameter is true.
        /// </summary>
        [Fact]
        public void ToJson_IndentedTrue_ContainsNewlines()
        {
            var factory = new PlanParserFactory();
            string json = factory.ToJson(indented: true);

            Assert.False(string.IsNullOrWhiteSpace(json));
            // Indented JSON should contain line breaks.
            Assert.Contains("\n", json);
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.FromJson(System.String)"/> returns a factory when given valid JSON.
        /// </summary>
        [Fact]
        public void FromJson_ValidJson_ReturnsFactory()
        {
            var original = new PlanParserFactory();
            string json = original.ToJson();

            var deserialized = PlanParserFactoryJsonExtensions.FromJson(json);

            Assert.NotNull(deserialized);
            // Verify that the deserialized factory knows the same parser types.
            var originalNames = original.GetRegisteredParserNames().OrderBy(x => x);
            var deserializedNames = deserialized!.GetRegisteredParserNames().OrderBy(x => x);
            Assert.Equal(originalNames, deserializedNames);
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.FromJson(System.String)"/> throws an <see cref="ArgumentException"/> when the JSON is null, empty, or whitespace.
        /// </summary>
        /// <param name="json">The JSON string to test, which is null, empty, or whitespace.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FromJson_NullOrEmpty_ThrowsArgumentException(string json)
        {
            Assert.Throws<ArgumentException>(() => PlanParserFactoryJsonExtensions.FromJson(json!));
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.TryFromJson(System.String, out PlanParserFactory?)"/> returns true and a factory when given valid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndFactory()
        {
            var factory = new PlanParserFactory();
            string json = factory.ToJson();

            bool success = PlanParserFactoryJsonExtensions.TryFromJson(json, out var result);

            Assert.True(success);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.TryFromJson(System.String, out PlanParserFactory?)"/> returns false and null when given invalid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            string badJson = "this is not json";

            bool success = PlanParserFactoryJsonExtensions.TryFromJson(badJson, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that <see cref="PlanParserFactoryJsonExtensions.TryFromJson(System.String, out PlanParserFactory?)"/> throws an <see cref="ArgumentException"/> when the JSON is null, empty, or whitespace.
        /// </summary>
        /// <param name="json">The JSON string to test, which is null, empty, or whitespace.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryFromJson_NullOrEmpty_ThrowsArgumentException(string json)
        {
            Assert.Throws<ArgumentException>(() => PlanParserFactoryJsonExtensions.TryFromJson(json!, out var _));
        }
    }
}