using System;
using SqlIndexAdvisor.Core.Engine;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public class RecommendationEngineJsonExtensionsTests
    {
        [Fact]
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
        public void ToJson_NullEngine_ThrowsArgumentNullException()
        {
            // Arrange
            RecommendationEngine? engine = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => engine!.ToJson());
        }

        [Fact]
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
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecommendationEngineJsonExtensions.FromJson(json!));
        }

        [Fact]
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
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecommendationEngineJsonExtensions.TryFromJson(json!, out _));
        }
    }
}
