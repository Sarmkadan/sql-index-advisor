using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the RecommendationMerger class.
/// </summary>
public class RecommendationMergerTests
{
    [Fact]
    public void Merge_WithPrefixColumns_MergesCorrectly()
    {
        // Arrange: Two recommendations for the same table where one is a prefix of the other
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "IsActive" },
                IncludeColumns = new List<string> { "Email", "Name" },
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "FullScanWithFilterRule" }
            },
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "IsActive", "CreatedAt" },
                IncludeColumns = new List<string> { "TotalOrders" },
                EstimatedImpactPercent = 75.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "ExpensiveSortRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the wider one)
        var result = Assert.Single(merged);

        // Verify the wider index was kept
        Assert.Equal("Users", result.Table);
        Assert.Equal(new[] { "Country", "IsActive", "CreatedAt" }, result.KeyColumns);

        // Verify includes were merged
        Assert.Contains("Email", result.IncludeColumns);
        Assert.Contains("Name", result.IncludeColumns);
        Assert.Contains("TotalOrders", result.IncludeColumns);

        // Verify impact and confidence were preserved
        Assert.Equal(75.0, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify reasons were merged
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public void Merge_WithSameColumns_MergesCorrectly()
    {
        // Arrange: Two identical recommendations (same columns)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "Status", "OrderDate" },
                IncludeColumns = new List<string> { "Total" },
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.Medium,
                Reasons = new List<string> { "Rule1" }
            },
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "Status", "OrderDate" },
                IncludeColumns = new List<string> { "CustomerId" },
                EstimatedImpactPercent = 65.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "Rule2" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        Assert.Equal("Orders", result.Table);
        Assert.Equal(new[] { "Status", "OrderDate" }, result.KeyColumns);

        // Includes should be merged
        Assert.Contains("Total", result.IncludeColumns);
        Assert.Contains("CustomerId", result.IncludeColumns);

        // Max impact and confidence should be used
        Assert.Equal(65.0, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Reasons should be merged
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public void Merge_WithDifferentTables_DoesNotMerge()
    {
        // Arrange: Recommendations for different tables
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "Rule1" }
            },
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "Country" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "Rule2" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have both recommendations
        Assert.Equal(2, merged.Count);
    }

    [Fact]
    public void Merge_WithNonPrefixColumns_DoesNotMerge()
    {
        // Arrange: Recommendations with different key columns (not a prefix relationship)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "Name" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "Rule1" }
            },
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Status", "OrderDate" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.High,
                Reasons = new List<string> { "Rule2" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have both recommendations
        Assert.Equal(2, merged.Count);
    }

    [Fact]
    public void Merge_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var recommendations = new List<IndexRecommendation>();

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert
        Assert.Empty(merged);
    }

    [Fact]
    public void Merge_SingleRecommendation_ReturnsSame()
    {
        // Arrange
        var recommendation = new IndexRecommendation
        {
            Table = "Users",
            KeyColumns = new List<string> { "Id" },
            IncludeColumns = new List<string> { "Name" },
            EstimatedImpactPercent = 50.0,
            Confidence = Confidence.High,
            Reasons = new List<string> { "Rule1" }
        };
        var recommendations = new List<IndexRecommendation> { recommendation };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert
        var result = Assert.Single(merged);
        Assert.Same(recommendation, result);
    }
}