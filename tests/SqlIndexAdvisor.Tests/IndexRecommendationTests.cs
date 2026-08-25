using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Unit tests for <see cref="IndexRecommendation"/> covering constructor initialization,
/// index name generation via <see cref="IIndexRecommendation.SuggestedName(string?)"/>,
/// DDL generation via <see cref="IIndexRecommendation.ToCreateStatement(PlanDialect)"/>,
/// and storage of impact, cost, confidence, and reason properties.
/// </summary>
public class IndexRecommendationTests : IIndexRecommendationTests
{
    /// <summary>
    /// Shared fixture: a fully populated recommendation for the Users table with key columns
    /// UserId and Email, include columns Name and CreatedDate, high confidence, and two reasons.
    /// Used by the default SuggestedName and ToCreateStatement tests.
    /// </summary>
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = IndexRecommendationTestsConstants.TestTableUsers,
        KeyColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnUserId, IndexRecommendationTestsConstants.TestColumnEmail },
        IncludeColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnName, IndexRecommendationTestsConstants.TestColumnCreatedDate },
        EstimatedImpactPercent = IndexRecommendationTestsConstants.TestImpactPercentDefault,
        SourceNodeCost = IndexRecommendationTestsConstants.TestSourceNodeCostDefault,
        Confidence = Confidence.High,
        Reasons = new List<string> { "Missing index on Users table", "Frequent WHERE clause on UserId and Email" }
    };

    /// <summary>
    /// Verifies that a recommendation built with every required property set stores the Products
    /// table, ProductId key column, Name and Price include columns, impact percent, source node
    /// cost, medium confidence, and its reasons list exactly as supplied.
    /// </summary>
    [Fact]
    public void Constructor_WithRequiredProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationTestsConstants.TestTableProducts,
            KeyColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnProductId },
            IncludeColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnProductName, IndexRecommendationTestsConstants.TestColumnPrice },
            EstimatedImpactPercent = IndexRecommendationTestsConstants.TestImpactPercentProducts,
            SourceNodeCost = IndexRecommendationTestsConstants.TestSourceNodeCostProducts,
            Confidence = Confidence.Medium,
            Reasons = new List<string> { "Common query pattern" }
        };

        // Assert
        Assert.Equal(IndexRecommendationTestsConstants.TestTableProducts, recommendation.Table);
        Assert.Equal(new List<string> { IndexRecommendationTestsConstants.TestColumnProductId }, recommendation.KeyColumns);
        Assert.Equal(new List<string> { IndexRecommendationTestsConstants.TestColumnProductName, IndexRecommendationTestsConstants.TestColumnPrice }, recommendation.IncludeColumns);
        Assert.Equal(IndexRecommendationTestsConstants.TestImpactPercentProducts, recommendation.EstimatedImpactPercent);
        Assert.Equal(IndexRecommendationTestsConstants.TestSourceNodeCostProducts, recommendation.SourceNodeCost);
        Assert.Equal(Confidence.Medium, recommendation.Confidence);
        Assert.Equal(new List<string> { "Common query pattern" }, recommendation.Reasons);
    }

    /// <summary>
    /// Verifies that constructing a recommendation with an empty IncludeColumns list yields an
    /// empty (not null) IncludeColumns collection on the instance.
    /// </summary>
    [Fact]
    public void Constructor_WithEmptyIncludeColumns_InitializesCorrectly()
    {
        // Arrange & Act
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationTestsConstants.TestTableOrders,
            KeyColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnOrderId },
            IncludeColumns = new List<string>()
        };

        // Assert
        Assert.Empty(recommendation.IncludeColumns);
    }

    /// <summary>
    /// Verifies that SuggestedName combines the Users table with its UserId and Email key
    /// columns into the expected "IX_Users_UserId_Email" index name.
    /// </summary>
    [Fact]
    public void SuggestedName_WithValidTableAndColumns_ReturnsCorrectFormat()
    {
        // Act
        var suggestedName = _testRecommendation.SuggestedName();

        // Assert
        Assert.Equal("IX_Users_UserId_Email", suggestedName);
    }

    /// <summary>
    /// Verifies that SuggestedName strips the "Sales." schema prefix from "Sales.Orders" and
    /// produces "IX_Orders_OrderId_CustomerId" from the remaining table and key columns.
    /// </summary>
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

    /// <summary>
    /// Verifies that SuggestedName keeps underscores in table names intact, rendering
    /// "dbo.User_Details" as "User_Details" in "IX_User_Details_UserId".
    /// </summary>
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

    /// <summary>
    /// Verifies that SuggestedName removes non-alphanumeric characters from column names,
    /// turning "User-Id" into "UserId" and "Email@domain.com" into "Emaildomaincom".
    /// </summary>
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

    /// <summary>
    /// Verifies that SuggestedName produces "IX_Customers_CustomerId" when the recommendation
    /// has only one key column.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToCreateStatement renders a SQL Server CREATE INDEX statement for the Users
    /// fixture listing both key columns and appending an INCLUDE clause with the cover columns.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToCreateStatement renders a Postgres CREATE INDEX statement containing only
    /// the key column list and no INCLUDE clause when no include columns are set.
    /// </summary>
    [Fact]
    public void ToCreateStatement_WithOnlyKeyColumns_ReturnsCorrectSql()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationTestsConstants.TestTableProducts,
            KeyColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnProductId }
        };
        var dialect = PlanDialect.Postgres;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Products_ProductId ON dbo.Products (ProductId);", createStatement);
    }

    /// <summary>
    /// Verifies that ToCreateStatement emits empty key-column parentheses plus an INCLUDE clause
    /// when KeyColumns is empty and only TotalAmount is provided as an include column.
    /// </summary>
    [Fact]
    public void ToCreateStatement_WithOnlyIncludeColumns_ReturnsCorrectSql()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationTestsConstants.TestTableOrders,
            KeyColumns = new List<string>(),
            IncludeColumns = new List<string> { IndexRecommendationTestsConstants.TestColumnTotalAmount }
        };
        var dialect = PlanDialect.SqlServer;

        // Act
        var createStatement = recommendation.ToCreateStatement(dialect);

        // Assert
        Assert.Equal("CREATE INDEX IX_Orders_ ON dbo.Orders () INCLUDE (TotalAmount);", createStatement);
    }

    /// <summary>
    /// Verifies that ToCreateStatement performs no validation of the Table property and still
    /// returns a statement with blank table and index-name segments when Table is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToCreateStatement throws ArgumentNullException when KeyColumns is null,
    /// because the underlying SuggestedName call rejects a null key column collection.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToCreateStatement throws NullReferenceException when IncludeColumns is null,
    /// since the statement builder dereferences it without a null check.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToCreateStatement lists all three key columns comma-separated in the
    /// generated statement for a Transactions recommendation that also carries two include columns.
    /// </summary>
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

    /// <summary>
    /// Verifies that SuggestedName uses only the final segment of a multi-part table name,
    /// producing "IX_Details_OrderDetailId" for the table "Sales.Order.Details".
    /// </summary>
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

    /// <summary>
    /// Verifies that SuggestedName preserves digits within table names, yielding
    /// "IX_User2_UserId" for the table "dbo.User2".
    /// </summary>
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

    /// <summary>
    /// Verifies that passing the default PlanDialect value to ToCreateStatement does not throw
    /// and still returns a non-null statement starting with "CREATE INDEX".
    /// </summary>
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

    /// <summary>
    /// Verifies that EstimatedImpactPercent accepts and stores the lower boundary value of 0.0
    /// without alteration.
    /// </summary>
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

    /// <summary>
    /// Verifies that EstimatedImpactPercent accepts and stores the upper boundary value of 100.0
    /// without alteration.
    /// </summary>
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

    /// <summary>
    /// Verifies that SourceNodeCost stores an arbitrary fractional cost value such as 0.5
    /// exactly as assigned.
    /// </summary>
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

    /// <summary>
    /// Verifies that separate recommendations store each Confidence enum value
    /// (Low, Medium, High) exactly as assigned.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Reasons property stores all supplied reason strings, preserving their
    /// original order.
    /// </summary>
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

    /// <summary>
    /// Verifies that assigning an empty Reasons list leaves the property as an empty collection
    /// rather than null.
    /// </summary>
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
