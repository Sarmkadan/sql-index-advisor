using SqlIndexAdvisor.Core.Model;
using static SqlIndexAdvisor.Tests.IndexRecommendationExtensionsTestsConstants;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Unit tests for the <see cref="IndexRecommendation"/> extension methods, covering
/// <c>ContainsColumn</c>, <c>GetTotalColumnCount</c>, <c>ToDisplayString</c> and <c>ToSummaryString</c>.
/// </summary>
public class IndexRecommendationExtensionsTests : IIndexRecommendationExtensionsTests
{
    /// <summary>
    /// A fully populated recommendation used as the shared fixture for most tests.
    /// </summary>
    private readonly IndexRecommendation _testRecommendation = new()
    {
        Table = TableUsers,
        KeyColumns = new List<string> { ColumnUserId, ColumnEmail },
        IncludeColumns = new List<string> { ColumnName, ColumnCreatedDate },
        EstimatedImpactPercent = EstimatedImpactPercentHigh,
        Confidence = Confidence.High,
        Reasons = new List<string> { ReasonMissingIndex, ReasonFrequentWhereClause }
    };

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> returns true when the requested column exists in the key columns.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithExistingKeyColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn(ColumnUserId);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> returns true when the requested column exists in the include columns.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithExistingIncludeColumn_ReturnsTrue()
    {
        // Act
        var result = _testRecommendation.ContainsColumn(ColumnName);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> returns false when the requested column is neither a key nor an include column.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithNonExistingColumn_ReturnsFalse()
    {
        // Act
        var result = _testRecommendation.ContainsColumn("NonExistentColumn");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> matches key and include columns case-insensitively.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> still finds a column listed only in the include columns
    /// when the key column collection is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>GetTotalColumnCount</c> returns the combined count of two key and two include columns.
    /// </summary>
    [Fact]
    public void GetTotalColumnCount_WithBothKeyAndIncludeColumns_ReturnsCorrectCount()
    {
        // Act
        var result = _testRecommendation.GetTotalColumnCount();

        // Assert
        Assert.Equal(4, result); // 2 key + 2 include
    }

    /// <summary>
    /// Verifies that <c>GetTotalColumnCount</c> returns the number of key columns when no include columns are defined.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>GetTotalColumnCount</c> returns the number of include columns when no key columns are defined.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>GetTotalColumnCount</c> returns zero when both the key and include column collections are empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToDisplayString</c> renders the index name, table, key and include column lists,
    /// the estimated impact percentage and the confidence level.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToDisplayString</c> renders a single-column key list without an INCLUDE clause
    /// when no include columns are defined.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToDisplayString</c> renders "(none)" for the missing key columns, the INCLUDE clause
    /// and the impact percentage when only include columns are defined.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToSummaryString</c> contains the suggested index name, table, key and include columns
    /// and the impact percentage.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToSummaryString</c> omits the INCLUDE clause when only key columns are defined.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToSummaryString</c> lists the include columns and the impact percentage when
    /// include columns accompany a single key column.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>ToSummaryString</c> produces the exact expected string for a recommendation with
    /// a single key column and no include columns.
    /// </summary>
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

    /// <summary>
    /// Verifies that calling <c>ContainsColumn</c> on a null recommendation throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ContainsColumn(ColumnUserId));
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> throws an <see cref="ArgumentNullException"/> when the column name is null.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithNullColumnName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _testRecommendation.ContainsColumn(null!));
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> throws an <see cref="ArgumentException"/> when the column name is empty.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithEmptyColumnName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _testRecommendation.ContainsColumn(""));
    }

    /// <summary>
    /// Verifies that <c>ContainsColumn</c> accepts whitespace-only column names without throwing and
    /// returns false because no such column exists.
    /// </summary>
    [Fact]
    public void ContainsColumn_WithWhitespaceColumnName_DoesNotThrow()
    {
        // Act & Assert - whitespace-only strings are not considered empty by ArgumentException.ThrowIfNullOrEmpty
        var result = _testRecommendation.ContainsColumn("   ");
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that calling <c>GetTotalColumnCount</c> on a null recommendation throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GetTotalColumnCount_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.GetTotalColumnCount());
    }

    /// <summary>
    /// Verifies that calling <c>ToDisplayString</c> on a null recommendation throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ToDisplayString_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ToDisplayString());
    }

    /// <summary>
    /// Verifies that calling <c>ToSummaryString</c> on a null recommendation throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ToSummaryString_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.ToSummaryString());
    }
}
