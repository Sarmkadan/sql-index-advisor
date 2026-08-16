using SqlIndexAdvisor.Core.Model;
using System.Text.Json;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationJsonExtensionsTests
{
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = IndexRecommendationJsonExtensionsTestsConstants.TableUsers,
        KeyColumns = new List<string> { IndexRecommendationJsonExtensionsTestsConstants.ColumnUserId, IndexRecommendationJsonExtensionsTestsConstants.ColumnEmail },
        IncludeColumns = new List<string> { IndexRecommendationJsonExtensionsTestsConstants.ColumnName, IndexRecommendationJsonExtensionsTestsConstants.ColumnCreatedDate },
        EstimatedImpactPercent = IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent85_5,
        SourceNodeCost = IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_75,
        Confidence = Confidence.High,
        Reasons = new List<string> { IndexRecommendationJsonExtensionsTestsConstants.ReasonMissingIndexOnUsersTable, IndexRecommendationJsonExtensionsTestsConstants.ReasonFrequentWhereClause }
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
        Assert.Contains($"\"table\":\"{IndexRecommendationJsonExtensionsTestsConstants.TableUsers}\"", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsKeyColumns()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains($"\"keyColumns\":[\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnUserId}\",\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnEmail}\"]", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsIncludeColumns()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains($"\"includeColumns\":[\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnName}\",\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnCreatedDate}\"]", result);
    }

    [Fact]
    public void ToJson_WithValidRecommendation_ContainsEstimatedImpactPercent()
    {
        // Act
        var result = _testRecommendation.ToJson();

        // Assert
        Assert.Contains($"\"estimatedImpactPercent\":{IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent85_5}", result);
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
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ReasonMissingIndexOnUsersTable, result);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ReasonFrequentWhereClause, result);
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
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.TableUsers, result.Table);
        Assert.Equal(2, result.KeyColumns.Count);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnUserId, result.KeyColumns);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnEmail, result.KeyColumns);
        Assert.Equal(2, result.IncludeColumns.Count);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnName, result.IncludeColumns);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnCreatedDate, result.IncludeColumns);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent85_5, result.EstimatedImpactPercent);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_75, result.SourceNodeCost);
        Assert.Equal(Confidence.High, result.Confidence);
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public void FromJson_WithValidJsonStringWithCamelCase_ReturnsIndexRecommendation()
    {
        // Arrange - JSON uses camelCase property names, enums are serialized as numbers
        var json = $"{{\r\n            \"table\": \"{IndexRecommendationJsonExtensionsTestsConstants.TableProducts}\",\r\n            \"keyColumns\": [\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnProductId}\", \"{IndexRecommendationJsonExtensionsTestsConstants.ColumnCategoryId}\"],\r\n            \"includeColumns\": [\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnProductName}\"],\r\n            \"estimatedImpactPercent\": {IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent72_3},\r\n            \"sourceNodeCost\": {IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_5},\r\n            \"confidence\": {(int)Confidence.Medium},\r\n            \"reasons\": [\"{IndexRecommendationJsonExtensionsTestsConstants.ReasonMissingIndex}\", \"{IndexRecommendationJsonExtensionsTestsConstants.ReasonPerformanceIssue}\"]\r\n        }}";

        // Act
        var result = IndexRecommendationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.TableProducts, result.Table);
        Assert.Equal(2, result.KeyColumns.Count);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnProductId, result.KeyColumns);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnCategoryId, result.KeyColumns);
        Assert.Single(result.IncludeColumns);
        Assert.Contains(IndexRecommendationJsonExtensionsTestsConstants.ColumnProductName, result.IncludeColumns);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent72_3, result.EstimatedImpactPercent);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_5, result.SourceNodeCost);
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
        var invalidJson = IndexRecommendationJsonExtensionsTestsConstants.InvalidJson;

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
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.TableUsers, value.Table);
        Assert.Equal(Confidence.High, value.Confidence);
    }

    [Fact]
    public void TryFromJson_WithValidJsonStringWithCamelCase_ReturnsTrueAndSetsValue()
    {
        // Arrange - JSON uses camelCase property names, enums are serialized as numbers
        var json = $"{{\"table\":\"{IndexRecommendationJsonExtensionsTestsConstants.TableOrders}\",\"keyColumns\":[\"{IndexRecommendationJsonExtensionsTestsConstants.ColumnOrderId}\"],\"includeColumns\":[],\"estimatedImpactPercent\":{IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent45_2},\"sourceNodeCost\":{IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_3},\"confidence\":{(int)Confidence.Low},\"reasons\":[]}}";

        // Act
        var result = IndexRecommendationJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.TableOrders, value.Table);
        Assert.Single(value.KeyColumns);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.ColumnOrderId, value.KeyColumns[0]);
        Assert.Empty(value.IncludeColumns);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent45_2, value.EstimatedImpactPercent);
        Assert.Equal(IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0_3, value.SourceNodeCost);
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
        var whitespaceJson = IndexRecommendationJsonExtensionsTestsConstants.WhitespaceJson;

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
        var invalidJson = IndexRecommendationJsonExtensionsTestsConstants.InvalidJson;

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
            Table = IndexRecommendationJsonExtensionsTestsConstants.TableMinimal,
            KeyColumns = new List<string> { IndexRecommendationJsonExtensionsTestsConstants.ColumnId },
            IncludeColumns = new List<string>(),
            EstimatedImpactPercent = IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent0,
            SourceNodeCost = IndexRecommendationJsonExtensionsTestsConstants.SourceNodeCost0,
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
                Table = IndexRecommendationJsonExtensionsTestsConstants.TableTest,
                KeyColumns = new List<string> { IndexRecommendationJsonExtensionsTestsConstants.ColumnId },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = IndexRecommendationJsonExtensionsTestsConstants.EstimatedImpactPercent50,
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
