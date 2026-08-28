using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for validating index recommendations.
/// </summary>
public class IndexRecommendationValidationTests : IIndexRecommendationValidationTests
{
    /// <summary>
    /// Tests that validating a valid index recommendation returns an empty list of errors.
    /// </summary>
    [Fact]
    public void Validate_WithValidRecommendation_ReturnsEmptyList()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId", "Email" },
            IncludeColumns = new List<string> { "Name", "CreatedDate" },
            EstimatedImpactPercent = 85.5,
            Confidence = Confidence.High,
            Reasons = new List<string> { "Missing index on Users table", "Frequent WHERE clause on UserId and Email" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with a null table returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithNullTable_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = null,
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorTableMustBeNonEmpty, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with an empty table returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyTable_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.InvalidString,
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorTableMustBeNonEmpty, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with null key columns returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithNullKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorKeyColumnsNotNull, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with empty key columns returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string>()
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorKeyColumnsMustContainAtLeastOne, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with whitespace key columns returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId", IndexRecommendationValidationTestsConstants.InvalidString, "Email" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorKeyColumnsMustBeNonEmpty, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with null include columns does not add an error.
    /// </summary>
    [Fact]
    public void Validate_WithNullIncludeColumns_DoesNotAddError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            IncludeColumns = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with whitespace include columns returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceIncludeColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            IncludeColumns = new List<string> { "Name", IndexRecommendationValidationTestsConstants.InvalidString, "CreatedDate" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorIncludeColumnsMustBeNonEmpty, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with invalid estimated impact percent returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithInvalidEstimatedImpactPercent_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = -1
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorEstimatedImpactPercentRange, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with maximum estimated impact percent returns no error.
    /// </summary>
    [Fact]
    public void Validate_WithMaxEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = IndexRecommendationValidationTestsConstants.MaxEstimatedImpactPercent
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with null reasons does not add an error.
    /// </summary>
    [Fact]
    public void Validate_WithNullReasons_DoesNotAddError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            Reasons = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with whitespace reasons returns an error.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceReasons_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            Reasons = new List<string> { "Valid reason", IndexRecommendationValidationTestsConstants.InvalidString, "Another valid reason" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal(IndexRecommendationValidationTestsConstants.ErrorReasonsMustBeNonEmpty, result[0]);
    }

    /// <summary>
    /// Tests that validating an index recommendation with multiple problems returns all errors.
    /// </summary>
    [Fact]
    public void Validate_WithMultipleProblems_ReturnsAllErrors()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.InvalidString,
            KeyColumns = new List<string> { "UserId", IndexRecommendationValidationTestsConstants.InvalidString },
            IncludeColumns = new List<string> { "Name", IndexRecommendationValidationTestsConstants.InvalidString },
            EstimatedImpactPercent = 150,
            Reasons = new List<string> { "Valid reason", IndexRecommendationValidationTestsConstants.InvalidString }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorTableMustBeNonEmpty, result);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorKeyColumnsMustBeNonEmpty, result);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorIncludeColumnsMustBeNonEmpty, result);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorEstimatedImpactPercentRange, result);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorReasonsMustBeNonEmpty, result);
    }

    /// <summary>
    /// Tests that IsValid returns true for a valid index recommendation.
    /// </summary>
    [Fact]
    public void IsValid_WithValidRecommendation_ReturnsTrue()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 50
        };

        // Act
        var result = recommendation.IsValid();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsValid returns false for an invalid index recommendation.
    /// </summary>
    [Fact]
    public void IsValid_WithInvalidRecommendation_ReturnsFalse()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.InvalidString,
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var result = recommendation.IsValid();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that IsValid throws an ArgumentNullException when the recommendation is null.
    /// </summary>
    [Fact]
    public void IsValid_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.IsValid());
    }

    /// <summary>
    /// Tests that EnsureValid does not throw for a valid index recommendation.
    /// </summary>
    [Fact]
    public void EnsureValid_WithValidRecommendation_DoesNotThrow()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 50
        };

        // Act
        var exception = Record.Exception(() => recommendation.EnsureValid());

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that EnsureValid throws an ArgumentException for an invalid index recommendation.
    /// </summary>
    [Fact]
    public void EnsureValid_WithInvalidRecommendation_ThrowsArgumentException()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.InvalidString,
            KeyColumns = new List<string> { "UserId" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => recommendation.EnsureValid());
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorIndexRecommendationInvalid, exception.Message);
        Assert.Contains(IndexRecommendationValidationTestsConstants.ErrorTableMustBeNonEmpty, exception.Message);
    }

    /// <summary>
    /// Tests that EnsureValid throws an ArgumentNullException when the recommendation is null.
    /// </summary>
    [Fact]
    public void EnsureValid_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.EnsureValid());
    }

    /// <summary>
    /// Tests that Validate throws an ArgumentNullException when the recommendation is null.
    /// </summary>
    [Fact]
    public void Validate_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.Validate());
    }

    /// <summary>
    /// Tests that validating an index recommendation with minimum estimated impact percent returns no error.
    /// </summary>
    [Fact]
    public void Validate_WithMinEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = IndexRecommendationValidationTestsConstants.MinEstimatedImpactPercent
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with boundary minimum estimated impact percent returns no error.
    /// </summary>
    [Fact]
    public void Validate_WithBoundaryEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 0.01
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that validating an index recommendation with boundary maximum estimated impact percent returns no error.
    /// </summary>
    [Fact]
    public void Validate_WithBoundaryMaxEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = IndexRecommendationValidationTestsConstants.DefaultTableName,
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 99.99
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }
}