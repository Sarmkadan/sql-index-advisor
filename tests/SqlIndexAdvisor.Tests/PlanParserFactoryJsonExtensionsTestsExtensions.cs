using System;
using System.Globalization;
using System.Linq;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Provides additional extension methods for <see cref="PlanParserFactoryJsonExtensionsTests"/>
    /// to facilitate testing of JSON serialization/deserialization behavior.
    /// </summary>
    public static class PlanParserFactoryJsonExtensionsTestsExtensions
    {
        /// <summary>
        /// Asserts that the JSON representation of a factory instance is valid and can be round-tripped.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="factory">The factory to test.</param>
        /// <param name="expectedParserCount">The expected number of registered parsers.</param>
        public static void JsonRoundtrip_PreservesParserRegistry(
            this PlanParserFactoryJsonExtensionsTests test,
            PlanParserFactory factory,
            int expectedParserCount)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(factory);

            string json = factory.ToJson(indented: true);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var deserialized = PlanParserFactoryJsonExtensions.FromJson(json);
            Assert.NotNull(deserialized);

            var originalNames = factory.GetRegisteredParserNames().OrderBy(x => x).ToList();
            var deserializedNames = deserialized!.GetRegisteredParserNames().OrderBy(x => x).ToList();

            Assert.Equal(expectedParserCount, originalNames.Count);
            Assert.Equal(expectedParserCount, deserializedNames.Count);
            Assert.Equal(originalNames, deserializedNames);
        }

        /// <summary>
        /// Asserts that JSON serialization produces consistent output across multiple calls.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="factory">The factory to test.</param>
        public static void ToJson_IsDeterministic(
            this PlanParserFactoryJsonExtensionsTests test,
            PlanParserFactory factory)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(factory);

            string json1 = factory.ToJson();
            string json2 = factory.ToJson();

            Assert.Equal(json1, json2);
        }

        /// <summary>
        /// Asserts that indented JSON contains expected structural elements.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="factory">The factory to test.</param>
        public static void ToJson_Indented_ContainsExpectedStructure(
            this PlanParserFactoryJsonExtensionsTests test,
            PlanParserFactory factory)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(factory);

            string json = factory.ToJson(indented: true);

            // Verify basic JSON structure
            Assert.StartsWith("{", json, StringComparison.Ordinal);
            Assert.EndsWith("}", json, StringComparison.Ordinal);

            // Verify we have some content between braces
            Assert.True(json.Length > 2);

            // Verify it's properly formatted with newlines
            string[] lines = json.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.NotEmpty(lines);
            Assert.True(lines.Length > 1, "Indented JSON should span multiple lines");
        }

        /// <summary>
        /// Asserts that non-indented JSON is compact and doesn't contain unnecessary whitespace.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="factory">The factory to test.</param>
        public static void ToJson_NonIndented_IsCompact(
            this PlanParserFactoryJsonExtensionsTests test,
            PlanParserFactory factory)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(factory);

            string json = factory.ToJson(indented: false);

            // Should not contain newlines
            Assert.DoesNotContain("\n", json);
            Assert.DoesNotContain("\r", json);

            // Should not contain excessive whitespace
            // A reasonable upper bound for compact JSON representation
            Assert.True(json.Length < 500, "JSON representation should be reasonably compact");
        }
    }
}