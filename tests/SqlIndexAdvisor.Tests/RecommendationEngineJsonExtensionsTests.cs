using System;
using SqlIndexAdvisor.Core.Engine;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Test class for verifying the JSON extension methods of the RecommendationEngine class.
    /// Contains tests for serialization to JSON and deserialization from JSON.
    /// </summary>
    public class RecommendationEngineJsonExtensionsTests : IRecommendationEngineJsonExtensionsTests
    {
        [Fact]
        /// <summary>
        /// Verifies that calling ToJson on a valid RecommendationEngine instance returns a non-empty JSON string.
        /// For an empty engine the default (non-indented) JSON is expected to be "{}".
        /// </summary>
        public void ToJson_WithValidEngine_ReturnsNonEmptyJson()
        {
            // Arrange
            var engine = new RecommendationEngine();

            // Act
            var json = engine.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            // For an empty object the default (non‑indented) JSON is "{}"
            Assert.Equal("{}", json.Trim());
        }

        [Fact]
        /// <summary>
        /// Verifies that calling ToJson with indented: true on a RecommendationEngine produces indented JSON.
        /// For an empty object, indented JSON contains newline characters.
        /// </summary>
        public void ToJson_WithIndentation_ProducesIndentedJson()
        {
            // Arrange
            var engine = new RecommendationEngine();

            // Act
            var json = engine.ToJson(indented: true);

            // Assert
            // Indented JSON for an empty object contains a newline character.
            Assert.Contains("\n", json);
        }

        [Fact]
        /// <summary>
        /// Verifies that calling ToJson on a null RecommendationEngine throws an ArgumentNullException.
        /// </summary>
        public void ToJson_NullEngine_ThrowsArgumentNullException()
        {
            // Arrange
            RecommendationEngine? engine = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => engine!.ToJson());
        }

        [Fact]
        /// <summary>
        /// Verifies that FromJson successfully deserializes valid JSON into a RecommendationEngine instance.
        /// The returned instance should be of type RecommendationEngine.
        /// </summary>
        public void FromJson_ValidJson_ReturnsEngineInstance()
        {
            // Arrange
            var json = "{}";

            // Act
            var engine = RecommendationEngineJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(engine);
            // The returned instance should be of the correct type.
            Assert.IsType<RecommendationEngine>(engine);
        }

        [Fact]
        /// <summary>
        /// Verifies that FromJson returns null when given an empty string or a string containing only whitespace.
        /// </summary>
        public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
        {
            // Arrange
            var empty = "";
            var whitespace = "   \n\t";

            // Act
            var resultEmpty = RecommendationEngineJsonExtensions.FromJson(empty);
            var resultWhite = RecommendationEngineJsonExtensions.FromJson(whitespace);

            // Assert
            Assert.Null(resultEmpty);
            Assert.Null(resultWhite);
        }

        [Fact]
        /// <summary>
        /// Verifies that FromJson throws an ArgumentNullException when given a null JSON string.
        /// </summary>
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecommendationEngineJsonExtensions.FromJson(json!));
        }

        [Fact]
        /// <summary>
        /// Verifies that TryFromJson returns true and a valid RecommendationEngine instance when given valid JSON.
        /// The out parameter should contain an instance of RecommendationEngine.
        /// </summary>
        public void TryFromJson_ValidJson_ReturnsTrueAndEngine()
        {
            // Arrange
            var json = "{}";

            // Act
            var success = RecommendationEngineJsonExtensions.TryFromJson(json, out var engine);

            // Assert
            Assert.True(success);
            Assert.NotNull(engine);
            Assert.IsType<RecommendationEngine>(engine);
        }

        [Fact]
        /// <summary>
        /// Verifies that TryFromJson returns false and a null engine when given invalid JSON.
        /// </summary>
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "not a json";

            // Act
            var success = RecommendationEngineJsonExtensions.TryFromJson(json, out var engine);

            // Assert
            Assert.False(success);
            Assert.Null(engine);
        }

        [Fact]
        /// <summary>
        /// Verifies that TryFromJson throws an ArgumentNullException when given a null JSON string.
        /// </summary>
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecommendationEngineJsonExtensions.TryFromJson(json!, out _));
        }
    }
}
