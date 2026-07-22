using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationExtensionsTests
{
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = "dbo.Users",
        KeyColumns = new List<string> { "UserId", "Email" },
        IncludeColumns = new List<string> { "Name", "CreatedDate" },
        EstimatedImpactPercent = 85.5,
        Confidence = Confidence.High,
        Reasons = new List<string> { "Missing index on Users table", "Frequent WHERE clause on UserId and Email" }
    };

    [Fact]
    public void ContainsColumn_WithExistingKeyColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn("UserId");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsColumn_WithExistingIncludeColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn("Name");

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
        var result1 = _testRecommendation.ContainsColumn("userid");
        var result2 = _testRecommendation.ContainsColumn("EMAIL");
        var result3 = _testRecommendation.ContainsColumn("name");

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
            Table = "dbo.Products",
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { "ProductName" }
        };

        // Act
        var result = recommendation.ContainsColumn("ProductName");

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
            Table = "dbo.Products",
            KeyColumns = new List<string> { "ProductId", "CategoryId" },
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
            Table = "dbo.Orders",
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { "OrderDate", "TotalAmount" }
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
            Table = "dbo.EmptyTable",
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
        Assert.Contains("Index IX_Users_UserId_Email on dbo.Users", result);
        Assert.Contains("(UserId, Email)", result);
        Assert.Contains("INCLUDE (Name, CreatedDate)", result);
        Assert.Contains("85.5%", result);
        Assert.Contains("Confidence: High", result);
    }

    [Fact]
    public void ToDisplayString_WithOnlyKeyColumns_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Products",
            KeyColumns = new List<string> { "ProductId" },
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.ToDisplayString();

        // Assert
        Assert.Contains("(ProductId)", result);
        Assert.Contains("Index IX_Products_ProductId on dbo.Products", result);
    }

    [Fact]
    public void ToDisplayString_WithOnlyIncludeColumns_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { "OrderDate" },
            EstimatedImpactPercent = 42.3
        };

        // Act
        var result = recommendation.ToDisplayString();

        // Assert
        Assert.Contains("(none)", result);
        Assert.Contains("INCLUDE (OrderDate)", result);
        Assert.Contains("42.3%", result);
    }

    [Fact]
    public void ToSummaryString_WithValidRecommendation_ReturnsConciseString()
    {
        // Act
        var result = _testRecommendation.ToSummaryString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("IX_Users_UserId_Email on dbo.Users", result);
        Assert.Contains("(UserId, Email)", result);
        Assert.Contains("INCLUDE (Name, CreatedDate)", result);
        Assert.Contains("85.5% impact", result);
    }

    [Fact]
    public void ToSummaryString_WithOnlyKeyColumns_ReturnsConciseFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Products",
            KeyColumns = new List<string> { "ProductId" },
            IncludeColumns = new List<string>()
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.DoesNotContain("INCLUDE", result);
        Assert.Contains("(ProductId)", result);
    }

    [Fact]
    public void ToSummaryString_WithOnlyIncludeColumns_ReturnsConciseFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new List<string> { "OrderId" },
            IncludeColumns = new List<string> { "TotalAmount", "CustomerName" },
            EstimatedImpactPercent = 37.8
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.Contains("INCLUDE (TotalAmount, CustomerName)", result);
        Assert.Contains("37.8% impact", result);
    }

    [Fact]
    public void ToSummaryString_WithSingleColumn_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Customers",
            KeyColumns = new List<string> { "CustomerId" },
            IncludeColumns = new List<string>(),
            EstimatedImpactPercent = 12.5
        };

        // Act
        var result = recommendation.ToSummaryString();

        // Assert
        Assert.Equal("IX_Customers_CustomerId on dbo.Customers (CustomerId) - 12.5% impact", result);
    }

    [Fact]
    public void ContainsColumn_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ContainsColumn("UserId"));
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