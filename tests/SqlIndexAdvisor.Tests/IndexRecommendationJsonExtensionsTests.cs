using SqlIndexAdvisor.Core.Model;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationJsonExtensionsTests
{
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = "dbo.Users",
        KeyColumns = new List<string> { "UserId", "Email" },
        IncludeColumns = new List<string> { "Name", "CreatedDate" },
        EstimatedImpactPercent = 85.5,
        SourceNodeCost = 0.75,
        Confidence = Confidence.High,
        Reasons = new List<string> { "Missing index on Users table", "Frequent WHERE clause on UserId and Email" }
    };

    [Fact]
    public void ToJson_WithValidRecommendation_ReturnsJsonString()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsTableName()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains("\"table\":\"dbo.Users\"", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsKeyColumns()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains("\"keyColumns\":[\"UserId\",\"Email\"]", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsIncludeColumns()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains("\"includeColumns\":[\"Name\",\"CreatedDate\"]", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsEstimatedImpactPercent()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains("\"estimatedImpactPercent\":85.5", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsConfidence()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert - enums are serialized as numbers by default (High = 2)
        Assert.Contains("\"confidence\":2", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsReasons()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains("\"reasons\":[", result);
        Assert.Contains("Missing index on Users table", result);
        Assert.Contains("Frequent WHERE clause on UserId and Email", result);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Act
        var result = _testRecommendation.ToJson(indented: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Contains("\n") || result.Contains("\r\n"), "Indented JSON should contain newlines");
    }

    [Fact]
    public void ToJson_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation? recommendation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJsonString_ReturnsIndexRecommendation()
    {
        // Arrange
        var json = _testRecommendation.ToJson();

        // Act
        var result = IndexRecommendationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dbo.Users", result.Table);
        Assert.Equal(2, result.KeyColumns.Count);
        Assert.Contains("UserId", result.KeyColumns);
        Assert.Contains("Email", result.KeyColumns);
        Assert.Equal(2, result.IncludeColumns.Count);
        Assert.Contains("Name", result.IncludeColumns);
        Assert.Contains("CreatedDate", result.IncludeColumns);
        Assert.Equal(85.5, result.EstimatedImpactPercent);
        Assert.Equal(0.75, result.SourceNodeCost);
        Assert.Equal(Confidence.High, result.Confidence);
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public void FromJson_WithValidJsonStringWithCamelCase_ReturnsIndexRecommendation()
    {
        // Arrange - JSON uses camelCase property names, enums are serialized as numbers
        var json = "{\r\n            \"table\": \"dbo.Products\",\r\n            \"keyColumns\": [\"ProductId\", \"CategoryId\"],\r\n            \"includeColumns\": [\"ProductName\"],\r\n            \"estimatedImpactPercent\": 72.3,\r\n            \"sourceNodeCost\": 0.5,\r\n            \"confidence\": 1,\r\n            \"reasons\": [\"Missing index\", \"Performance issue\"]\r\n        }";

        // Act
        var result = IndexRecommendationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dbo.Products", result.Table);
        Assert.Equal(2, result.KeyColumns.Count);
        Assert.Contains("ProductId", result.KeyColumns);
        Assert.Contains("CategoryId", result.KeyColumns);
        Assert.Single(result.IncludeColumns);
        Assert.Contains("ProductName", result.IncludeColumns);
        Assert.Equal(72.3, result.EstimatedImpactPercent);
        Assert.Equal(0.5, result.SourceNodeCost);
        Assert.Equal(Confidence.Medium, result.Confidence);
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public void FromJson_WithNullJsonString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => IndexRecommendationJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_WithEmptyJsonString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IndexRecommendationJsonExtensions.FromJson(""));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act & Assert
        Assert.Throws<JsonException>(() => IndexRecommendationJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_WithValidJsonString_ReturnsTrueAndSetsValue()
    {
        // Arrange
        var json = _testRecommendation.ToJson();

        // Act
        var result = IndexRecommendationJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal("dbo.Users", value.Table);
        Assert.Equal(Confidence.High, value.Confidence);
    }

    [Fact]
    public void TryFromJson_WithValidJsonStringWithCamelCase_ReturnsTrueAndSetsValue()
    {
        // Arrange - JSON uses camelCase property names, enums are serialized as numbers
        var json = "{\"table\":\"dbo.Orders\",\"keyColumns\":[\"OrderId\"],\"includeColumns\":[],\"estimatedImpactPercent\":45.2,\"sourceNodeCost\":0.3,\"confidence\":0,\"reasons\":[]}";

        // Act
        var result = IndexRecommendationJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal("dbo.Orders", value.Table);
        Assert.Single(value.KeyColumns);
        Assert.Equal("OrderId", value.KeyColumns[0]);
        Assert.Empty(value.IncludeColumns);
        Assert.Equal(45.2, value.EstimatedImpactPercent);
        Assert.Equal(0.3, value.SourceNodeCost);
        Assert.Equal(Confidence.Low, value.Confidence);
    }

    [Fact]
    public void TryFromJson_WithNullJsonString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => IndexRecommendationJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_WithEmptyJsonString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IndexRecommendationJsonExtensions.TryFromJson("", out _));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJsonString_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange - whitespace-only strings pass ArgumentException.ThrowIfNullOrEmpty check
        var whitespaceJson = "   ";

        // Act
        var result = IndexRecommendationJsonExtensions.TryFromJson(whitespaceJson, out var value);

        // Assert - whitespace is not valid JSON, so TryFromJson returns false
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        var result = IndexRecommendationJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void RoundTrip_WithValidRecommendation_PreservesAllData()
    {
        // Arrange
        var original = _testRecommendation;

        // Act - serialize and deserialize
        var json = original.ToJson();
        var deserialized = IndexRecommendationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Table, deserialized.Table);
        Assert.Equal(original.KeyColumns.Count, deserialized.KeyColumns.Count);
        for (int i = 0; i < original.KeyColumns.Count; i++)
        {
            Assert.Equal(original.KeyColumns[i], deserialized.KeyColumns[i]);
        }
        Assert.Equal(original.IncludeColumns.Count, deserialized.IncludeColumns.Count);
        for (int i = 0; i < original.IncludeColumns.Count; i++)
        {
            Assert.Equal(original.IncludeColumns[i], deserialized.IncludeColumns[i]);
        }
        Assert.Equal(original.EstimatedImpactPercent, deserialized.EstimatedImpactPercent);
        Assert.Equal(original.SourceNodeCost, deserialized.SourceNodeCost);
        Assert.Equal(original.Confidence, deserialized.Confidence);
        Assert.Equal(original.Reasons.Count, deserialized.Reasons.Count);
        for (int i = 0; i < original.Reasons.Count; i++)
        {
            Assert.Equal(original.Reasons[i], deserialized.Reasons[i]);
        }
    }

    [Fact]
    public void RoundTrip_WithMinimalRecommendation_PreservesData()
    {
        // Arrange - minimal recommendation
        var minimal = new IndexRecommendation
        {
            Table = "dbo.Minimal",
            KeyColumns = new List<string> { "Id" },
            IncludeColumns = new List<string>(),
            EstimatedImpactPercent = 0.0,
            SourceNodeCost = 0.0,
            Confidence = Confidence.Low,
            Reasons = new List<string>()
        };

        // Act - serialize and deserialize
        var json = minimal.ToJson();
        var deserialized = IndexRecommendationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(minimal.Table, deserialized.Table);
        Assert.Equal(minimal.KeyColumns.Count, deserialized.KeyColumns.Count);
        Assert.Equal(minimal.IncludeColumns.Count, deserialized.IncludeColumns.Count);
        Assert.Equal(minimal.EstimatedImpactPercent, deserialized.EstimatedImpactPercent);
        Assert.Equal(minimal.SourceNodeCost, deserialized.SourceNodeCost);
        Assert.Equal(minimal.Confidence, deserialized.Confidence);
        Assert.Equal(minimal.Reasons.Count, deserialized.Reasons.Count);
    }

    [Fact]
    public void RoundTrip_WithAllConfidenceLevels_PreservesConfidence()
    {
        // Test all confidence levels
        var confidenceLevels = new[] { Confidence.Low, Confidence.Medium, Confidence.High };

        foreach (var confidence in confidenceLevels)
        {
            // Arrange
            var recommendation = new IndexRecommendation
            {
                Table = "dbo.Test",
                KeyColumns = new List<string> { "Id" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 50.0,
                Confidence = confidence,
                Reasons = new List<string>()
            };

            // Act
            var json = recommendation.ToJson();
            var deserialized = IndexRecommendationJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(confidence, deserialized.Confidence);
        }
    }
}