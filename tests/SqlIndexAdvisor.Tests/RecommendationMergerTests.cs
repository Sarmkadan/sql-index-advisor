using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the RecommendationMerger class.
/// </summary>
public class RecommendationMergerTests : IRecommendationMergerTests
{
    [Fact]
    public void Merge_WithPrefixColumns_MergesCorrectly()
    {
        // Arrange: Two recommendations for the same table where one is a prefix of the other
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn, RecommendationMergerTestsConstants.IsActiveColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.EmailColumn, RecommendationMergerTestsConstants.NameColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.FullScanWithFilterRuleReason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn, RecommendationMergerTestsConstants.IsActiveColumn, RecommendationMergerTestsConstants.CreatedAtColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.TotalOrdersColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.PeakImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.ExpensiveSortRuleReason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the wider one)
        var result = Assert.Single(merged);

        // Verify the wider index was kept
        Assert.Equal(RecommendationMergerTestsConstants.UsersTable, result.Table);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.CountryColumn, RecommendationMergerTestsConstants.IsActiveColumn, RecommendationMergerTestsConstants.CreatedAtColumn }, result.KeyColumns);

        // Verify includes were merged
        Assert.Contains(RecommendationMergerTestsConstants.EmailColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.NameColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.TotalOrdersColumn, result.IncludeColumns);

        // Verify impact and confidence were preserved
        Assert.Equal(RecommendationMergerTestsConstants.PeakImpactPercent, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify reasons were merged
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedMergedReasonCount, result.Reasons.Count);
    }

    [Fact]
    public void Merge_WithSameColumns_MergesCorrectly()
    {
        // Arrange: Two identical recommendations (same columns)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.OrdersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.StatusColumn, RecommendationMergerTestsConstants.OrderDateColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.TotalColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.ModerateImpactPercent,
                Confidence = Confidence.Medium,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.OrdersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.StatusColumn, RecommendationMergerTestsConstants.OrderDateColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.CustomerIdColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.HighImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        Assert.Equal(RecommendationMergerTestsConstants.OrdersTable, result.Table);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.StatusColumn, RecommendationMergerTestsConstants.OrderDateColumn }, result.KeyColumns);

        // Includes should be merged
        Assert.Contains(RecommendationMergerTestsConstants.TotalColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.CustomerIdColumn, result.IncludeColumns);

        // Max impact and confidence should be used
        Assert.Equal(RecommendationMergerTestsConstants.HighImpactPercent, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Reasons should be merged
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedMergedReasonCount, result.Reasons.Count);
    }

    [Fact]
    public void Merge_WithDifferentTables_DoesNotMerge()
    {
        // Arrange: Recommendations for different tables
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.OrdersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.ModerateImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have both recommendations
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedUnmergedCount, merged.Count);
    }

    [Fact]
    public void Merge_WithNonPrefixColumns_DoesNotMerge()
    {
        // Arrange: Recommendations with different key columns (not a prefix relationship)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn, RecommendationMergerTestsConstants.NameColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.StatusColumn, RecommendationMergerTestsConstants.OrderDateColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.ModerateImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have both recommendations
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedUnmergedCount, merged.Count);
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
            Table = RecommendationMergerTestsConstants.UsersTable,
            KeyColumns = new List<string> { RecommendationMergerTestsConstants.IdColumn },
            IncludeColumns = new List<string> { RecommendationMergerTestsConstants.NameColumn },
            EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
            Confidence = Confidence.High,
            Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
        };
        var recommendations = new List<IndexRecommendation> { recommendation };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert
        var result = Assert.Single(merged);
        Assert.Same(recommendation, result);
    }

    [Fact]
    public void Merge_WithReorderedColumns_DoesNotMerge()
    {
        // Arrange: Recommendations with same columns but different order (not a prefix relationship)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CountryColumn, RecommendationMergerTestsConstants.NameColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.UsersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.NameColumn, RecommendationMergerTestsConstants.CountryColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.ModerateImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have both recommendations (reordered columns are not considered matching)
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedUnmergedCount, merged.Count);
    }

    [Fact]
    public void Merge_WithIncludeColumnOverlap_MergesCorrectly()
    {
        // Arrange: Two recommendations with overlapping include columns
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.OrdersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CustomerIdColumn, RecommendationMergerTestsConstants.OrderDateColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.TotalAmountColumn, RecommendationMergerTestsConstants.StatusColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.Medium,
                Reasons = new List<string> { RecommendationMergerTestsConstants.KeyLookupRuleReason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.OrdersTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CustomerIdColumn, RecommendationMergerTestsConstants.OrderDateColumn, RecommendationMergerTestsConstants.ProductIdColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.TotalAmountColumn, RecommendationMergerTestsConstants.QuantityColumn, RecommendationMergerTestsConstants.PriceColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.PeakImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.FullScanWithFilterRuleReason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the wider one)
        var result = Assert.Single(merged);

        Assert.Equal(RecommendationMergerTestsConstants.OrdersTable, result.Table);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.CustomerIdColumn, RecommendationMergerTestsConstants.OrderDateColumn, RecommendationMergerTestsConstants.ProductIdColumn }, result.KeyColumns);

        // Verify includes were merged and deduplicated
        Assert.Contains(RecommendationMergerTestsConstants.TotalAmountColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.StatusColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.QuantityColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.PriceColumn, result.IncludeColumns);
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedMergedIncludeCount, result.IncludeColumns.Count);

        // Verify impact and confidence were preserved
        Assert.Equal(RecommendationMergerTestsConstants.PeakImpactPercent, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);
    }

    [Fact]
    public void Merge_WithEmptyIncludeColumns_MergesCorrectly()
    {
        // Arrange: One recommendation with include columns, one without
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.ProductsTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CategoryColumn, RecommendationMergerTestsConstants.PriceColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.NameColumn, RecommendationMergerTestsConstants.DescriptionColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.LowerImpactPercent,
                Confidence = Confidence.Medium,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.ProductsTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.CategoryColumn, RecommendationMergerTestsConstants.PriceColumn, RecommendationMergerTestsConstants.StockQuantityColumn },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = RecommendationMergerTestsConstants.ModerateImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        Assert.Equal(RecommendationMergerTestsConstants.ProductsTable, result.Table);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.CategoryColumn, RecommendationMergerTestsConstants.PriceColumn, RecommendationMergerTestsConstants.StockQuantityColumn }, result.KeyColumns);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.NameColumn, RecommendationMergerTestsConstants.DescriptionColumn }, result.IncludeColumns);
        Assert.Equal(RecommendationMergerTestsConstants.ModerateImpactPercent, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);
    }

    [Fact]
    public void Merge_MultiplePrefixRelationships_MergesAll()
    {
        // Arrange: Three recommendations where each is a prefix of the next
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.SalesTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.RegionColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.CountColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.LowImpactPercent,
                Confidence = Confidence.Low,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule1Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.SalesTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.RegionColumn, RecommendationMergerTestsConstants.YearColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.QuarterColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.BaseImpactPercent,
                Confidence = Confidence.Medium,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule2Reason }
            },
            new IndexRecommendation
            {
                Table = RecommendationMergerTestsConstants.SalesTable,
                KeyColumns = new List<string> { RecommendationMergerTestsConstants.RegionColumn, RecommendationMergerTestsConstants.YearColumn, RecommendationMergerTestsConstants.MonthColumn },
                IncludeColumns = new List<string> { RecommendationMergerTestsConstants.DayColumn },
                EstimatedImpactPercent = RecommendationMergerTestsConstants.MaxImpactPercent,
                Confidence = Confidence.High,
                Reasons = new List<string> { RecommendationMergerTestsConstants.Rule3Reason }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the widest one)
        var result = Assert.Single(merged);

        Assert.Equal(RecommendationMergerTestsConstants.SalesTable, result.Table);
        Assert.Equal(new[] { RecommendationMergerTestsConstants.RegionColumn, RecommendationMergerTestsConstants.YearColumn, RecommendationMergerTestsConstants.MonthColumn }, result.KeyColumns);

        // Verify all include columns were merged
        Assert.Contains(RecommendationMergerTestsConstants.CountColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.QuarterColumn, result.IncludeColumns);
        Assert.Contains(RecommendationMergerTestsConstants.DayColumn, result.IncludeColumns);
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedMultiMergeIncludeCount, result.IncludeColumns.Count);

        // Verify max impact and confidence were used
        Assert.Equal(RecommendationMergerTestsConstants.MaxImpactPercent, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify all reasons were merged (3 original + 2 heuristic notes from 2 merges)
        Assert.Equal(RecommendationMergerTestsConstants.ExpectedMultiMergeReasonCount, result.Reasons.Count);
    }
}
