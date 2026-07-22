using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationTests
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
    public void Constructor_WithRequiredProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Products",
            KeyColumns = new List<string> { "ProductId" },
            IncludeColumns = new List<string> { "ProductName", "Price" },
            EstimatedImpactPercent = 90.0,
            SourceNodeCost = 0.80,
            Confidence = Confidence.Medium,
            Reasons = new List<string> { "Common query pattern" }
        };

        // Assert
        Assert.Equal("dbo.Products", recommendation.Table);
        Assert.Equal(new List<string> { "ProductId" }, recommendation.KeyColumns);
        Assert.Equal(new List<string> { "ProductName", "Price" }, recommendation.IncludeColumns);
        Assert.Equal(90.0, recommendation.EstimatedImpactPercent);
        Assert.Equal(0.80, recommendation.SourceNodeCost);
        Assert.Equal(Confidence.Medium, recommendation.Confidence);
        Assert.Equal(new List<string> { "Common query pattern" }, recommendation.Reasons);
    }

    [Fact]
    public void Constructor_WithEmptyIncludeColumns_InitializesCorrectly()
    {
        // Arrange & Act
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new List<string> { "OrderId" },
            IncludeColumns = new List<string>()
        };

        // Assert
        Assert.Empty(recommendation.IncludeColumns);
    }

    [Fact]
    public void SuggestedName_WithValidTableAndColumns_ReturnsCorrectFormat()
    {
        // Act
        var suggestedName = _testRecommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Users_UserId_Email", suggestedName);
    }

    [Fact]
    public void SuggestedName_WithSchemaQualifiedTable_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "Sales.Orders",
            KeyColumns = new List<string> { "OrderId", "CustomerId" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Orders_OrderId_CustomerId", suggestedName);
    }

    [Fact]
    public void SuggestedName_WithSpecialCharactersInTableName_SanitizesCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.User_Details",
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_User_Details_UserId", suggestedName);
    }

    [Fact]
    public void SuggestedName_WithSpecialCharactersInColumns_SanitizesCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "User-Id", "Email@domain.com" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Users_UserId_Emaildomaincom", suggestedName);
    }

    [Fact]
    public void SuggestedName_WithSingleColumn_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Customers",
            KeyColumns = new List<string> { "CustomerId" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Customers_CustomerId", suggestedName);
    }

    [Fact]
    public void ToCreateStatement_WithKeyAndIncludeColumns_ReturnsCorrectSql()
    {
        // Arrange
        var dialect = PlanDialect.SqlServer;

        // Act
        var createStatement = _testRecommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Users_UserId_Email ON dbo.Users (UserId, Email) INCLUDE (Name, CreatedDate);", createStatement);
    }

    [Fact]
    public void ToCreateStatement_WithOnlyKeyColumns_ReturnsCorrectSql()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Products",
            KeyColumns = new List<string> { "ProductId" }
        };
        var dialect = PlanDialect.Postgres;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Products_ProductId ON dbo.Products (ProductId);", createStatement);
    }

    [Fact]
    public void ToCreateStatement_WithOnlyIncludeColumns_ReturnsCorrectSql()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { "TotalAmount" }
        };
        var dialect = PlanDialect.SqlServer;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Orders_ ON dbo.Orders () INCLUDE (TotalAmount);", createStatement);
    }

    [Fact]
    public void ToCreateStatement_WithEmptyTableName_ReturnsStatementWithEmptyTable()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "",
            KeyColumns = new List<string> { "Column1" }
        };
        var dialect = PlanDialect.SqlServer;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert - The method doesn't validate Table property, so it creates the statement anyway
        Assert.Equal("CREATE INDEX IX__Column1 ON  (Column1);", createStatement);
    }

    [Fact]
    public void ToCreateStatement_WithNullKeyColumns_ThrowsArgumentNullException()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = null!
        };
        var dialect = PlanDialect.SqlServer;

        // Act & Assert - SuggestedName() throws ArgumentNullException when KeyColumns is null
        Assert.Throws<ArgumentNullException>(() => recommendation.ToCreateStatement(dialect));
    }

    [Fact]
    public void ToCreateStatement_WithNullIncludeColumns_ThrowsNullReferenceException()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            IncludeColumns = null!
        };
        var dialect = PlanDialect.SqlServer;

        // Act & Assert - ToCreateStatement doesn't validate IncludeColumns, so it throws NullReferenceException
        Assert.Throws<NullReferenceException>(() => recommendation.ToCreateStatement(dialect));
    }

    [Fact]
    public void ToCreateStatement_WithMultipleKeyColumns_ReturnsCorrectSql()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Transactions",
            KeyColumns = new List<string> { "TransactionId", "AccountId", "TransactionDate" },
            IncludeColumns = new List<string> { "Amount", "Status" }
        };
        var dialect = PlanDialect.Postgres;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Transactions_TransactionId_AccountId_TransactionDate ON dbo.Transactions (TransactionId, AccountId, TransactionDate) INCLUDE (Amount, Status);", createStatement);
    }

    [Fact]
    public void SuggestedName_WithSchemaInTableName_ExtractsTableNameCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "Sales.Order.Details",
            KeyColumns = new List<string> { "OrderDetailId" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Details_OrderDetailId", suggestedName);
    }

    [Fact]
    public void SuggestedName_WithNumbersInTableName_ReturnsCorrectFormat()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.User2",
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var suggestedName = recommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_User2_UserId", suggestedName);
    }

    [Fact]
    public void ToCreateStatement_WithNullDialect_DoesNotThrow()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" }
        };

        // Act
        var createStatement = recommendation.ToCreateStatement(default);

        // Assert
        Assert.NotNull(createStatement);
        Assert.Contains("CREATE INDEX", createStatement);
    }

    [Fact]
    public void EstimatedImpactPercent_WithBoundaryValues_StoresCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            EstimatedImpactPercent = 0.0
        };

        // Assert
        Assert.Equal(0.0, recommendation.EstimatedImpactPercent);
    }

    [Fact]
    public void EstimatedImpactPercent_WithMaximumValue_StoresCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            EstimatedImpactPercent = 100.0
        };

        // Assert
        Assert.Equal(100.0, recommendation.EstimatedImpactPercent);
    }

    [Fact]
    public void SourceNodeCost_WithValidValue_StoresCorrectly()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            SourceNodeCost = 0.5
        };

        // Assert
        Assert.Equal(0.5, recommendation.SourceNodeCost);
    }

    [Fact]
    public void Confidence_WithAllValues_StoresCorrectly()
    {
        // Arrange & Act & Assert for each value
        var low = new IndexRecommendation { Table = "dbo.Test", KeyColumns = new List<string> { "Col1" }, Confidence = Confidence.Low };
        Assert.Equal(Confidence.Low, low.Confidence);

        var medium = new IndexRecommendation { Table = "dbo.Test", KeyColumns = new List<string> { "Col1" }, Confidence = Confidence.Medium };
        Assert.Equal(Confidence.Medium, medium.Confidence);

        var high = new IndexRecommendation { Table = "dbo.Test", KeyColumns = new List<string> { "Col1" }, Confidence = Confidence.High };
        Assert.Equal(Confidence.High, high.Confidence);
    }

    [Fact]
    public void Reasons_WithMultipleReasons_StoresCorrectly()
    {
        // Arrange
        var reasons = new List<string> { "Reason1", "Reason2", "Reason3" };
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            Reasons = reasons
        };

        // Assert
        Assert.Equal(reasons, recommendation.Reasons);
    }

    [Fact]
    public void Reasons_WithEmptyList_InitializesEmptyList()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Test",
            KeyColumns = new List<string> { "Col1" },
            Reasons = new List<string>()
        };

        // Assert
        Assert.Empty(recommendation.Reasons);
    }
}