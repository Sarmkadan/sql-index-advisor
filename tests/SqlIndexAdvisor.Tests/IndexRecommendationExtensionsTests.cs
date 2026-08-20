using SqlIndexAdvisor.Core.Model;
using static SqlIndexAdvisor.Tests.IndexRecommendationExtensionsTestsConstants;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationExtensionsTests : IIndexRecommendationExtensionsTests
{
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = TableUsers,
        KeyColumns = new List<string> { ColumnUserId, ColumnEmail },
        IncludeColumns = new List<string> { ColumnName, ColumnCreatedDate },
        EstimatedImpactPercent = EstimatedImpactPercentHigh,
        Confidence = Confidence.High,
        Reasons = new List<string> { ReasonMissingIndex, ReasonFrequentWhereClause }
    };

    [Fact]
    public void ContainsColumn_WithExistingKeyColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn(ColumnUserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsColumn_WithExistingIncludeColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn(ColumnName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsColumn_WithNonExistingColumn_ReturnsFalse()
    {
        // Act
        var result = _testRecommendation.ContainsColumn("NonExistentColumn");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsColumn_WithCaseInsensitiveMatch_ReturnsTrue()
    {
        // Act
        var result1 = _testRecommendation.ContainsColumn(ColumnUserId.ToLower());
        var result2 = _testRecommendation.ContainsColumn(ColumnEmail.ToUpper());
        var result3 = _testRecommendation.ContainsColumn(ColumnName.ToLower());

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    [Fact]
    public void ContainsColumn_WithEmptyKeyColumns_ReturnsFalse()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableProducts,
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { ColumnProductName }
        };

        // Act
        var result = recommendation.ContainsColumn(ColumnProductName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetTotalColumnCount_WithBothKeyAndIncludeColumns_ReturnsCorrectCount()
    {
        // Act
        var result = _testRecommendation.GetTotalColumnCount();

        // Assert
        Assert.Equal(4, result); // 2 key + 2 include
    }

    [Fact]
    public void GetTotalColumnCount_WithOnlyKeyColumns_ReturnsKeyCount()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableProducts,
            KeyColumns = new List<string> { ColumnProductId, ColumnCategoryId },
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.GetTotalColumnCount();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetTotalColumnCount_WithOnlyIncludeColumns_ReturnsIncludeCount()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableOrders,
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { ColumnOrderDate, ColumnTotalAmount }
        };

        // Act
        var result = recommendation.GetTotalColumnCount();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetTotalColumnCount_WithEmptyCollections_ReturnsZero()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableEmptyTable,
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.GetTotalColumnCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToDisplayString_WithValidRecommendation_ReturnsFormattedString()
    {
        // Act
        var result = _testRecommendation.ToDisplayString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Index IX_Users_UserId_Email on " + TableUsers, result);
        Assert.Contains($"({ColumnUserId}, {ColumnEmail})", result);
        Assert.Contains($"INCLUDE ({ColumnName}, {ColumnCreatedDate})", result);
        Assert.Contains($"{EstimatedImpactPercentHigh}%", result);
        Assert.Contains("Confidence: High", result);
    }

    [Fact]
    public void ToDisplayString_WithOnlyKeyColumns_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableProducts,
            KeyColumns = new List<string> { ColumnProductId },
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.ToDisplayString();

        // Assert
        Assert.Contains($"({ColumnProductId})", result);
        Assert.Contains($"Index IX_Products_{ColumnProductId} on " + TableProducts, result);
    }

    [Fact]
    public void ToDisplayString_WithOnlyIncludeColumns_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableOrders,
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { ColumnOrderDate },
            EstimatedImpactPercent = EstimatedImpactPercentMedium
        };

        // Act
        var result = recommendation.ToDisplayString();

        // Assert
        Assert.Contains("(none)", result);
        Assert.Contains($"INCLUDE ({ColumnOrderDate})", result);
        Assert.Contains($"{EstimatedImpactPercentMedium}%", result);
    }

    [Fact]
    public void ToSummaryString_WithValidRecommendation_ReturnsConciseString()
    {
        // Act
        var result = _testRecommendation.ToSummaryString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains($"IX_Users_{ColumnUserId}_{ColumnEmail} on " + TableUsers, result);
        Assert.Contains($"({ColumnUserId}, {ColumnEmail})", result);
        Assert.Contains($"INCLUDE ({ColumnName}, {ColumnCreatedDate})", result);
        Assert.Contains($"{EstimatedImpactPercentHigh}% impact", result);
    }

    [Fact]
    public void ToSummaryString_WithOnlyKeyColumns_ReturnsConciseFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableProducts,
            KeyColumns = new List<string> { ColumnProductId },
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.DoesNotContain("INCLUDE", result);
        Assert.Contains($"({ColumnProductId})", result);
    }

    [Fact]
    public void ToSummaryString_WithOnlyIncludeColumns_ReturnsConciseFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableOrders,
            KeyColumns = new List<string> { "OrderId" },
            IncludeColumns = new List<string> { ColumnTotalAmount, ColumnCustomerName },
            EstimatedImpactPercent = EstimatedImpactPercentMediumLow
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.Contains($"INCLUDE ({ColumnTotalAmount}, {ColumnCustomerName})", result);
        Assert.Contains($"{EstimatedImpactPercentMediumLow}% impact", result);
    }

    [Fact]
    public void ToSummaryString_WithSingleColumn_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = TableCustomers,
            KeyColumns = new List<string> { ColumnCustomerId },
            IncludeColumns = new List<string>(),
            EstimatedImpactPercent = EstimatedImpactPercentLow
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.Equal($"IX_Customers_{ColumnCustomerId} on {TableCustomers} ({ColumnCustomerId}) - {EstimatedImpactPercentLow}% impact", result);
    }

    [Fact]
    public void ContainsColumn_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ContainsColumn(ColumnUserId));
    }

    [Fact]
    public void ContainsColumn_WithNullColumnName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _testRecommendation.ContainsColumn(null!));
    }

    [Fact]
    public void ContainsColumn_WithEmptyColumnName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _testRecommendation.ContainsColumn(""));
    }

    [Fact]
    public void ContainsColumn_WithWhitespaceColumnName_DoesNotThrow()
    {
        // Act & Assert - whitespace-only strings are not considered empty by ArgumentException.ThrowIfNullOrEmpty
        var result = _testRecommendation.ContainsColumn("   ");
        Assert.False(result);
    }

    [Fact]
    public void GetTotalColumnCount_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.GetTotalColumnCount());
    }

    [Fact]
    public void ToDisplayString_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ToDisplayString());
    }

    [Fact]
    public void ToSummaryString_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ToSummaryString());
    }
}
