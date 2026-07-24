using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the RecommendationMerger conflict resolution and deduplication logic.
/// Tests scenarios where multiple rules produce recommendations for the same index,
/// requiring conflict resolution based on rule type, impact scores, and column relationships.
/// </summary>
public class RecommendationMergerConflictTests
{
    [Fact]
    public void Merge_WithOptimizerHintAndHeuristicRule_PrefersOptimizerHint()
    {
        // Arrange: One optimizer-native hint (engine-hint rule) and one heuristic rule
        // for the same table and columns. The optimizer hint should be preferred.
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId", "OrderDate" },
                IncludeColumns = new List<string> { "TotalAmount" },
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.Medium,
                Rule = "engine-hint",  // Optimizer-native hint with real impact
                Reasons = new List<string> { "EngineHintRule" }
            },
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId", "OrderDate" },
                IncludeColumns = new List<string> { "Status" },
                EstimatedImpactPercent = 45.0,
                Confidence = Confidence.High,
                Rule = "KeyLookupRule",  // Heuristic rule
                Reasons = new List<string> { "KeyLookupRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        // Verify the optimizer hint was kept
        Assert.Equal("Orders", result.Table);
        Assert.Equal(new[] { "CustomerId", "OrderDate" }, result.KeyColumns);
        Assert.Equal("engine-hint", result.Rule);

        // Verify includes were merged
        Assert.Contains("TotalAmount", result.IncludeColumns);
        Assert.Contains("Status", result.IncludeColumns);

        // Verify impact from optimizer hint was preserved
        Assert.Equal(50.0, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence); // Max confidence
    }

    [Fact]
    public void Merge_WithHeuristicAndOptimizerHint_PrefersOptimizerHint()
    {
        // Arrange: Heuristic rule first, then optimizer-native hint
        // The optimizer hint should still be preferred regardless of order
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Products",
                KeyColumns = new List<string> { "Category", "Price" },
                IncludeColumns = new List<string> { "Name" },
                EstimatedImpactPercent = 35.0,
                Confidence = Confidence.Medium,
                Rule = "MissingJoinIndexRule",
                Reasons = new List<string> { "MissingJoinIndexRule" }
            },
            new IndexRecommendation
            {
                Table = "Products",
                KeyColumns = new List<string> { "Category", "Price" },
                IncludeColumns = new List<string> { "Description" },
                EstimatedImpactPercent = 65.0,
                Confidence = Confidence.High,
                Rule = "engine-hint",  // Optimizer-native hint
                Reasons = new List<string> { "EngineHintRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        // Verify the optimizer hint was kept even though it came second
        Assert.Equal("engine-hint", result.Rule);
        Assert.Equal(65.0, result.EstimatedImpactPercent); // Impact from optimizer hint
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify includes were merged
        Assert.Contains("Name", result.IncludeColumns);
        Assert.Contains("Description", result.IncludeColumns);
    }

    [Fact]
    public void Merge_TwoOptimizerHints_PicksHigherImpact()
    {
        // Arrange: Two optimizer-native hints (both have Rule = "engine-hint")
        // The one with higher impact should be selected
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "IsActive" },
                IncludeColumns = new List<string> { "Email" },
                EstimatedImpactPercent = 45.5,
                Confidence = Confidence.Medium,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            },
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "IsActive" },
                IncludeColumns = new List<string> { "Name" },
                EstimatedImpactPercent = 72.8,
                Confidence = Confidence.High,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        // Verify the higher impact optimizer hint was kept
        Assert.Equal("engine-hint", result.Rule);
        Assert.Equal(72.8, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify includes were merged
        Assert.Contains("Email", result.IncludeColumns);
        Assert.Contains("Name", result.IncludeColumns);
    }

    [Fact]
    public void Merge_TwoOptimizerHintsWithSameImpact_PicksOne()
    {
        // Arrange: Two optimizer-native hints with same impact
        // Should pick one deterministically (the one that comes first in the merge)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Sales",
                KeyColumns = new List<string> { "Region", "Year" },
                IncludeColumns = new List<string> { "Count" },
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.High,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            },
            new IndexRecommendation
            {
                Table = "Sales",
                KeyColumns = new List<string> { "Region", "Year" },
                IncludeColumns = new List<string> { "Total" },
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.Medium,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        // Verify one of the optimizer hints was kept
        Assert.Equal("engine-hint", result.Rule);
        Assert.Equal(60.0, result.EstimatedImpactPercent);

        // Verify includes were merged
        Assert.Contains("Count", result.IncludeColumns);
        Assert.Contains("Total", result.IncludeColumns);
    }

    [Fact]
    public void Merge_HeuristicRulesWithoutOptimizerHint_MergesAsBefore()
    {
        // Arrange: Two heuristic rules (no optimizer hints)
        // Should merge as before, keeping the wider index
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Customers",
                KeyColumns = new List<string> { "Country" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 30.0,
                Confidence = Confidence.Low,
                Rule = "FullScanWithFilterRule",
                Reasons = new List<string> { "FullScanWithFilterRule" }
            },
            new IndexRecommendation
            {
                Table = "Customers",
                KeyColumns = new List<string> { "Country", "IsActive" },
                IncludeColumns = new List<string> { "Name" },
                EstimatedImpactPercent = 55.0,
                Confidence = Confidence.High,
                Rule = "MissingJoinIndexRule",
                Reasons = new List<string> { "MissingJoinIndexRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the wider one)
        var result = Assert.Single(merged);

        // Verify the wider index was kept
        Assert.Equal(new[] { "Country", "IsActive" }, result.KeyColumns);
        Assert.Equal(55.0, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);

        // Verify includes were merged
        Assert.Contains("Name", result.IncludeColumns);

        // Verify heuristic impact note was added
        Assert.Contains("Impact estimate is heuristic, not optimizer-reported.", result.Reasons);
    }

    [Fact]
    public void Merge_WithSchemaFixImplicitConversionColumn_FiltersOutCreateIndex()
    {
        // Arrange: A SchemaFix recommendation that identifies CustomerId as having implicit conversion,
        // and a CreateIndex recommendation that has CustomerId as a key column.
        // The CreateIndex should be filtered out because it includes CustomerId which was identified
        // as having an implicit conversion by the SchemaFix.
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.High,
                Kind = RecommendationKind.CreateIndex,
                Rule = "KeyLookupRule",
                Reasons = new List<string> { "KeyLookupRule" }
            },
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 10.0,
                Confidence = Confidence.Low,
                Kind = RecommendationKind.SchemaFix,
                Rule = "ImplicitConversionRule",
                Reasons = new List<string> { "ImplicitConversionRule: CustomerId has implicit conversion" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: The CreateIndex recommendation should be filtered out because CustomerId
        // was identified as having an implicit conversion by the SchemaFix
        Assert.Single(merged);
        var result = merged[0];

        // Should only have the SchemaFix recommendation
        Assert.Equal(RecommendationKind.SchemaFix, result.Kind);
        Assert.Equal("CustomerId", result.KeyColumns[0]);
    }

    [Fact]
    public void Merge_WithSchemaFixOnSameColumn_FiltersCreateIndexRecommendation()
    {
        // Arrange: SchemaFix identifies CustomerId as having implicit conversion,
        // CreateIndex also has CustomerId as a key column
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId", "OrderDate" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.High,
                Kind = RecommendationKind.CreateIndex,
                Rule = "KeyLookupRule",
                Reasons = new List<string> { "KeyLookupRule" }
            },
            new IndexRecommendation
            {
                Table = "Orders",
                KeyColumns = new List<string> { "CustomerId" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 5.0,
                Confidence = Confidence.Low,
                Kind = RecommendationKind.SchemaFix,
                Rule = "ImplicitConversionRule",
                Reasons = new List<string> { "ImplicitConversionRule: CustomerId has implicit conversion" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: The CreateIndex recommendation should be filtered out because it has CustomerId
        // which was identified as having an implicit conversion by the SchemaFix
        var result = Assert.Single(merged);
        Assert.Equal(RecommendationKind.SchemaFix, result.Kind);
        Assert.Equal("ImplicitConversionRule", result.Rule);
        // The result should be the CreateIndex with CustomerId and OrderDate, but filtered out
        // Actually, after filtering, only the SchemaFix remains
        Assert.Equal(new[] { "CustomerId" }, result.KeyColumns);
    }

    [Fact]
    public void Merge_WithMultipleSchemaFixes_FiltersAllAffectedCreateIndexRecommendations()
    {
        // Arrange: Multiple SchemaFix recommendations identifying different columns with conversions
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country", "Name" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 50.0,
                Confidence = Confidence.High,
                Kind = RecommendationKind.CreateIndex,
                Rule = "KeyLookupRule"
            },
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Status" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 40.0,
                Confidence = Confidence.Medium,
                Kind = RecommendationKind.SchemaFix,
                Rule = "ImplicitConversionRule",
                Reasons = new List<string> { "Status has implicit conversion" }
            },
            new IndexRecommendation
            {
                Table = "Users",
                KeyColumns = new List<string> { "Country" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 30.0,
                Confidence = Confidence.Low,
                Kind = RecommendationKind.SchemaFix,
                Rule = "ImplicitConversionRule",
                Reasons = new List<string> { "Country has implicit conversion" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Only the SchemaFix recommendations should remain
        Assert.Equal(2, merged.Count);
        Assert.All(merged, r => Assert.Equal(RecommendationKind.SchemaFix, r.Kind));
    }

    [Fact]
    public void Merge_NullRecommendationsList_ThrowsArgumentNullException()
    {
        // Arrange
        List<IndexRecommendation> recommendations = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RecommendationMerger.Merge(recommendations));
    }

    [Fact]
    public void Merge_WithPrefixColumnsAndOptimizerHint_PrefersWiderIndexWithOptimizerHint()
    {
        // Arrange: Prefix relationship where the narrower is an optimizer hint
        // The wider heuristic rule should be kept (wider index wins over optimizer hint)
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Products",
                KeyColumns = new List<string> { "Category", "Price" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 45.0,
                Confidence = Confidence.Medium,
                Rule = "KeyLookupRule",
                Reasons = new List<string> { "KeyLookupRule" }
            },
            new IndexRecommendation
            {
                Table = "Products",
                KeyColumns = new List<string> { "Category", "Price", "StockQuantity" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 60.0,
                Confidence = Confidence.High,
                Rule = "MissingJoinIndexRule",
                Reasons = new List<string> { "MissingJoinIndexRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation (the wider one)
        var result = Assert.Single(merged);

        // Verify the wider index was kept (wider wins over optimizer hint)
        Assert.Equal(new[] { "Category", "Price", "StockQuantity" }, result.KeyColumns);
        Assert.Equal(60.0, result.EstimatedImpactPercent);
        Assert.Equal(Confidence.High, result.Confidence);
    }

    [Fact]
    public void Merge_WithPrefixColumnsBothOptimizerHints_PicksHigherImpact()
    {
        // Arrange: Prefix relationship where both are optimizer hints
        // Should pick the one with higher impact
        var recommendations = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "Customers",
                KeyColumns = new List<string> { "Region" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 35.5,
                Confidence = Confidence.Medium,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            },
            new IndexRecommendation
            {
                Table = "Customers",
                KeyColumns = new List<string> { "Region", "Country" },
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = 72.3,
                Confidence = Confidence.High,
                Rule = "engine-hint",
                Reasons = new List<string> { "EngineHintRule" }
            }
        };

        // Act
        var merged = RecommendationMerger.Merge(recommendations);

        // Assert: Should have only one recommendation
        var result = Assert.Single(merged);

        // Verify the higher impact optimizer hint was kept
        Assert.Equal(new[] { "Region", "Country" }, result.KeyColumns);
        Assert.Equal(72.3, result.EstimatedImpactPercent);
        Assert.Equal("engine-hint", result.Rule);
    }
}