using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

public class IndexRecommendationValidationTests
{
    [Fact]
    public void Validate_WithValidRecommendation_ReturnsEmptyList()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
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
        Assert.Equal("Table property must be a non-empty string.", result[0]);
    }

    [Fact]
    public void Validate_WithEmptyTable_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "   ",
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("Table property must be a non-empty string.", result[0]);
    }

    [Fact]
    public void Validate_WithNullKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("KeyColumns collection must not be null.", result[0]);
    }

    [Fact]
    public void Validate_WithEmptyKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string>()
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("KeyColumns collection must contain at least one column.", result[0]);
    }

    [Fact]
    public void Validate_WithWhitespaceKeyColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId", "   ", "Email" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("All KeyColumns must be non-empty strings.", result[0]);
    }

    [Fact]
    public void Validate_WithNullIncludeColumns_DoesNotAddError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            IncludeColumns = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithWhitespaceIncludeColumns_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            IncludeColumns = new List<string> { "Name", "   ", "CreatedDate" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("All IncludeColumns must be non-empty strings.", result[0]);
    }

    [Fact]
    public void Validate_WithInvalidEstimatedImpactPercent_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = -1
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("EstimatedImpactPercent must be between 0 and 100 inclusive.", result[0]);
    }

    [Fact]
    public void Validate_WithMaxEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 100
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNullReasons_DoesNotAddError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            Reasons = null
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithWhitespaceReasons_ReturnsError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            Reasons = new List<string> { "Valid reason", "   ", "Another valid reason" }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Single(result);
        Assert.Equal("All Reasons must be non-empty strings.", result[0]);
    }

    [Fact]
    public void Validate_WithMultipleProblems_ReturnsAllErrors()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "   ",
            KeyColumns = new List<string> { "UserId", "   " },
            IncludeColumns = new List<string> { "Name", "   " },
            EstimatedImpactPercent = 150,
            Reasons = new List<string> { "Valid reason", "   " }
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Contains("Table property must be a non-empty string.", result);
        Assert.Contains("All KeyColumns must be non-empty strings.", result);
        Assert.Contains("All IncludeColumns must be non-empty strings.", result);
        Assert.Contains("EstimatedImpactPercent must be between 0 and 100 inclusive.", result);
        Assert.Contains("All Reasons must be non-empty strings.", result);
    }

    [Fact]
    public void IsValid_WithValidRecommendation_ReturnsTrue()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 50
        };

        // Act
        var result = recommendation.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithInvalidRecommendation_ReturnsFalse()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "   ",
            KeyColumns = new List<string> { "UserId" }
        };

        // Act
        var result = recommendation.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.IsValid());
    }

    [Fact]
    public void EnsureValid_WithValidRecommendation_DoesNotThrow()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 50
        };

        // Act
        var exception = Record.Exception(() => recommendation.EnsureValid());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_WithInvalidRecommendation_ThrowsArgumentException()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "   ",
            KeyColumns = new List<string> { "UserId" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => recommendation.EnsureValid());
        Assert.Contains("IndexRecommendation is invalid:", exception.Message);
        Assert.Contains("Table property must be a non-empty string.", exception.Message);
    }

    [Fact]
    public void EnsureValid_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.EnsureValid());
    }

    [Fact]
    public void Validate_WithNullRecommendation_ThrowsArgumentNullException()
    {
        // Arrange
        IndexRecommendation recommendation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => recommendation.Validate());
    }

    [Fact]
    public void Validate_WithMinEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 0
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithBoundaryEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 0.01
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithBoundaryMaxEstimatedImpactPercent_ReturnsNoError()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Users",
            KeyColumns = new List<string> { "UserId" },
            EstimatedImpactPercent = 99.99
        };

        // Act
        var result = recommendation.Validate();

        // Assert
        Assert.Empty(result);
    }
}