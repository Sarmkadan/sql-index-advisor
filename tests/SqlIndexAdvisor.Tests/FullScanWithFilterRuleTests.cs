using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for <see cref="FullScanWithFilterRule"/> which flags table scans with filter predicates
/// and recommends indexes with predicate columns as keys and output columns as includes.
/// </summary>
public class FullScanWithFilterRuleTests : IFullScanWithFilterRuleTests
{
    private readonly FullScanWithFilterRule _rule = new();

    [Fact]
    public void Evaluate_SeqScanWithFilterPredicate_ReturnsRecommendation()
    {
        // Arrange
        var plan = FullScanWithFilterRuleTestsConstants.UsersTable.CreateSeqScanPlan();

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.UsersTable, recommendation.Table);
        Assert.Equal(new[] { "id" }, recommendation.KeyColumns);
        Assert.Empty(recommendation.IncludeColumns);
        Assert.Equal(Confidence.High, recommendation.Confidence);
        Assert.Contains(FullScanWithFilterRuleTestsConstants.SeqScanUsersReason, recommendation.Reasons);
    }

    [Fact]
    public void Evaluate_ClusteredIndexScanWithFilterPredicate_ReturnsRecommendation()
    {
        // Arrange
        var plan = FullScanWithFilterRuleTestsConstants.OrdersTable.CreateClusteredIndexScanPlan();

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.OrdersTable, recommendation.Table);
        Assert.Equal(new[] { "status" }, recommendation.KeyColumns);
        Assert.Equal(new[] { "id", "total", "customer_id" }, recommendation.IncludeColumns);
        Assert.Equal(Confidence.High, recommendation.Confidence);
        Assert.Contains(FullScanWithFilterRuleTestsConstants.ClusteredIndexScanOrdersReason, recommendation.Reasons);
    }

    [Fact]
    public void Evaluate_SeqScanWithoutPredicate_ReturnsNoRecommendation()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = "products",
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.HighRelativeCost,
                    PredicateColumns = { } // No predicate columns
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_TableScanWithLowCost_ReturnsNoRecommendation()
    {
        // Arrange - scan with cost below MinRelativeCost threshold (0.10)
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Table Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.CustomersTable,
                    EstimatedRows = 100,
                    EstimatedRowsRead = 10000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.LowRelativeCost,
                    PredicateColumns = { "name" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_IndexScanWithFilterPredicate_ReturnsRecommendation()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Index Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.ProductsTable,
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.MediumRelativeCost,
                    PredicateColumns = { "category_id", "price" },
                    OutputColumns = { "id", "name", "category_id", "price", "stock" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.ProductsTable, recommendation.Table);
        Assert.Equal(new[] { "category_id", "price" }, recommendation.KeyColumns);
        Assert.Equal(new[] { "id", "name", "stock" }, recommendation.IncludeColumns);
        Assert.Equal(Confidence.Medium, recommendation.Confidence);
        Assert.Contains(FullScanWithFilterRuleTestsConstants.IndexScanProductsReason, recommendation.Reasons);
    }

    [Fact]
    public void Evaluate_MultipleScans_ReturnsRecommendationForScanWithFilterOnly()
    {
        // Arrange - multiple nodes, only one with predicate
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.UsersTable,
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.HighRelativeCost,
                    PredicateColumns = { } // No predicate - should be ignored
                },
                new()
                {
                    Operator = "Seq Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.OrdersTable,
                    EstimatedRows = 5000,
                    EstimatedRowsRead = 5000000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.MediumHighRelativeCost,
                    PredicateColumns = { "customer_id" } // Has predicate - should match
                },
                new()
                {
                    Operator = "Index Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.ProductsTable,
                    EstimatedRows = 200,
                    EstimatedRowsRead = 200000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.LowMediumRelativeCost,
                    PredicateColumns = { } // No predicate - should be ignored
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.OrdersTable, recommendation.Table);
        Assert.Equal(new[] { "customer_id" }, recommendation.KeyColumns);
    }

    [Fact]
    public void Evaluate_ScanWithHighCostAndSelectiveFilter_ReturnsHighConfidence()
    {
        // Arrange - high cost scan with selective filter (few output rows vs read rows)
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.LogsTable,
                    EstimatedRows = 100, // Only 100 rows output
                    EstimatedRowsRead = 1000000, // But read 1M rows
                    RelativeCost = FullScanWithFilterRuleTestsConstants.HighRelativeCost,
                    PredicateColumns = { "timestamp" },
                    OutputColumns = { "id", "message", "timestamp", "severity" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.LogsTable, recommendation.Table);
        Assert.Equal(new[] { "timestamp" }, recommendation.KeyColumns);
        Assert.Equal(new[] { "id", "message", "severity" }, recommendation.IncludeColumns);
        Assert.Equal(Confidence.High, recommendation.Confidence);
        Assert.InRange(recommendation.EstimatedImpactPercent, 36.0, 90.0); // 0.9 * 100 * (0.4 + 0.6 * 0.9999) = ~90
    }

    [Fact]
    public void Evaluate_ScanWithMediumCost_ReturnsMediumConfidence()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "transactions",
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 100000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.MediumLowRelativeCost,
                    PredicateColumns = { "status", "date" },
                    OutputColumns = { "id", "amount", "status", "date", "account_id" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal("transactions", recommendation.Table);
        Assert.Equal(new[] { "status", "date" }, recommendation.KeyColumns);
        Assert.Equal(new[] { "id", "amount", "account_id" }, recommendation.IncludeColumns);
        Assert.Equal(Confidence.Medium, recommendation.Confidence);
    }

    [Fact]
    public void Evaluate_ScanWithLowCost_ReturnsLowConfidence()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.AuditTable,
                    EstimatedRows = 50,
                    EstimatedRowsRead = 1000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.LowRelativeCost2,
                    PredicateColumns = { "user_id" },
                    OutputColumns = { "id", "action", "user_id", "timestamp" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal(FullScanWithFilterRuleTestsConstants.AuditTable, recommendation.Table);
        Assert.Equal(new[] { "user_id" }, recommendation.KeyColumns);
        Assert.Equal(new[] { "id", "action", "timestamp" }, recommendation.IncludeColumns);
        Assert.Equal(Confidence.Low, recommendation.Confidence);
    }

    [Fact]
    public void Evaluate_ScanWithoutTableName_ReturnsNoRecommendation()
    {
        // Arrange - scan without table name
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = null, // No table name
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.HighRelativeCost,
                    PredicateColumns = { "id" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_PlanWithZeroMatchingNodes_ReturnsEmptyRecommendationList()
    {
        // Arrange - plan with nodes that don't match any criteria
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Nested Loops",
                    TableName = FullScanWithFilterRuleTestsConstants.UsersTable,
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.MediumRelativeCost,
                    PredicateColumns = { } // No predicate columns
                },
                new()
                {
                    Operator = "Index Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.ProductsTable,
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.LowMediumRelativeCost,
                    PredicateColumns = { } // No predicate columns - Index Scan without predicate is not a full scan
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should return empty list without throwing
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_ScanWithCostExactlyAtThreshold_ReturnsRecommendation()
    {
        // Arrange - scan with cost exactly at MinRelativeCost threshold (0.10)
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Table Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.CustomersTable,
                    EstimatedRows = 100,
                    EstimatedRowsRead = 10000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.MinRelativeCost, // Exactly at threshold
                    PredicateColumns = { "name" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
    }

    [Fact]
    public void Evaluate_ScanWithCostJustBelowThreshold_ReturnsNoRecommendation()
    {
        // Arrange - scan with cost just below MinRelativeCost threshold
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Table Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.CustomersTable,
                    EstimatedRows = 100,
                    EstimatedRowsRead = 10000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.JustBelowMinRelativeCost, // Just below threshold
                    PredicateColumns = { "name" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_ScanWithCostJustAboveThreshold_ReturnsRecommendation()
    {
        // Arrange - scan with cost just above MinRelativeCost threshold
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = FullScanWithFilterRuleTestsConstants.EstimatedTotalCost,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Table Scan",
                    TableName = FullScanWithFilterRuleTestsConstants.CustomersTable,
                    EstimatedRows = 100,
                    EstimatedRowsRead = 10000,
                    RelativeCost = FullScanWithFilterRuleTestsConstants.JustAboveMinRelativeCost, // Just above threshold
                    PredicateColumns = { "name" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Single(recommendations);
    }

    [Fact]
    public void Name_ReturnsLowercaseRuleName()
    {
        // Act & Assert
        Assert.Equal(FullScanWithFilterRuleTestsConstants.RuleName, _rule.Name);
    }
}
