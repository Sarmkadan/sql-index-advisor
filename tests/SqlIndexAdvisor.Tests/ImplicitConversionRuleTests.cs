using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for <see cref="ImplicitConversionRule"/> which detects implicit conversions in SQL queries
/// that can prevent index usage. The rule flags CONVERT_IMPLICIT operations and type mismatches
/// and recommends aligning column/parameter types to enable index seeks.
/// </summary>
public class ImplicitConversionRuleTests
{
    private readonly ImplicitConversionRule _rule = new();

    [Fact]
    public void Evaluate_QueryWithImplicitConversionOnPredicate_ReturnsRecommendation()
    {
        // Arrange - SQL Server query with CONVERT_IMPLICIT function call in statement text
        // CONVERT_IMPLICIT appears in SQL Server execution plans when there's an implicit conversion
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM orders WHERE CONVERT_IMPLICIT(varchar(10), status) = 'completed'",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "orders",
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.8,
                    PredicateColumns = { "status" },
                    OutputColumns = { "id", "status", "amount", "customer_id" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Rule should detect implicit conversions and return recommendations
        Assert.NotEmpty(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal("orders", recommendation.Table);
        Assert.NotEmpty(recommendation.KeyColumns);
        Assert.Contains("status", recommendation.KeyColumns);
        Assert.Equal(Confidence.High, recommendation.Confidence);
        Assert.Contains("Query contains implicit conversion", recommendation.Reasons[0]);
    }


    [Fact]
    public void Evaluate_QueryWithImplicitConversionOnNonPredicateExpression_ReturnsNoRecommendation()
    {
        // Arrange - Query with implicit conversion in a non-predicate expression (e.g., in SELECT list)
        // This should NOT fire because the implicit conversion is not on a predicate column
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT id, CONVERT_IMPLICIT(varchar(10), status) AS status_text FROM orders WHERE id = 123",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Seek",
                    TableName = "orders",
                    EstimatedRows = 1,
                    EstimatedRowsRead = 1,
                    RelativeCost = 0.1,
                    PredicateColumns = { "id" },
                    OutputColumns = { "id", "status" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should not recommend since implicit conversion is not on a predicate column
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_QueryWithExplicitCast_ReturnsNoRecommendation()
    {
        // Arrange - Query with explicit CAST (not CONVERT_IMPLICIT)
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM orders WHERE CAST(status AS varchar(10)) = 'completed'",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "orders",
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.8,
                    PredicateColumns = { "status" },
                    OutputColumns = { "id", "status", "amount", "customer_id" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should not recommend since there's no CONVERT_IMPLICIT marker
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_PostgresQueryWithTypeMismatchCast_ReturnsRecommendation()
    {
        // Arrange - Postgres query with type mismatch cast using :: operator
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            StatementText = "SELECT * FROM users WHERE user_id::text = '123'",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "user_id" },
                    OutputColumns = { "id", "user_id", "name", "email" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should detect the :: cast and return recommendations
        Assert.NotEmpty(recommendations);
        var recommendation = recommendations[0];

        Assert.Equal("users", recommendation.Table);
        Assert.NotEmpty(recommendation.KeyColumns);
        Assert.Contains("user_id", recommendation.KeyColumns);
        Assert.Equal(Confidence.High, recommendation.Confidence);
        Assert.Contains("Query contains implicit conversion", recommendation.Reasons[0]);
    }

    [Fact]
    public void Evaluate_MultipleImplicitConversions_ReturnsRecommendationsForMultipleTables()
    {
        // Arrange - Query with CONVERT_IMPLICIT markers
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM orders JOIN customers ON orders.customer_id = customers.id",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "orders",
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.5,
                    PredicateColumns = { "status", "customer_id" },
                    OutputColumns = { "id", "status", "customer_id", "amount" }
                },
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "customers",
                    EstimatedRows = 1,
                    EstimatedRowsRead = 1000000,
                    RelativeCost = 0.4,
                    PredicateColumns = { "id" },
                    OutputColumns = { "id", "name", "email" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should get recommendations for tables in the plan
        Assert.NotEmpty(recommendations);
        Assert.Contains(recommendations, r => r.Table == "orders");
        Assert.Contains(recommendations, r => r.Table == "customers");
    }

    [Fact]
    public void Evaluate_PlanWithoutImplicitConversion_ReturnsNoRecommendation()
    {
        // Arrange - Query with no CONVERT_IMPLICIT marker
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM orders WHERE status = 'completed'",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Seek",
                    TableName = "orders",
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500,
                    RelativeCost = 0.3,
                    PredicateColumns = { "status" },
                    OutputColumns = { "id", "status", "amount", "customer_id" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_PlanWithNoTables_ReturnsNoRecommendation()
    {
        // Arrange - Plan with no table references
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT 1",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>()
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should return empty since there are no tables to associate conversions with
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_EmptyPlan_ReturnsNoRecommendation()
    {
        // Arrange - Empty plan
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT 1",
            EstimatedTotalCost = 0,
            Nodes = new List<PlanNode>()
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert - Should return empty list without throwing
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Evaluate_QueryWithMultipleImplicitConversionsOnDifferentColumns_ReturnsMultipleColumnRecommendations()
    {
        // Arrange - Query with multiple CONVERT_IMPLICIT operations on different columns
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM orders WHERE CONVERT_IMPLICIT(varchar(10), status) = 'completed' AND CONVERT_IMPLICIT(varchar(20), customer_id) = '123'",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>
            {
                new()
                {
                    Operator = "Clustered Index Scan",
                    TableName = "orders",
                    EstimatedRows = 500,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.8,
                    PredicateColumns = { "status", "customer_id" },
                    OutputColumns = { "id", "status", "customer_id", "amount" }
                }
            }
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.NotEmpty(recommendations);
        var recommendation = recommendations[0];
        Assert.Equal("orders", recommendation.Table);
        Assert.Equal(2, recommendation.KeyColumns.Count);
        Assert.Contains("status", recommendation.KeyColumns);
        Assert.Contains("customer_id", recommendation.KeyColumns);
    }

    [Fact]
    public void Evaluate_QueryWithImplicitConversionButNoMatchingTables_ReturnsNoRecommendation()
    {
        // Arrange - Query with CONVERT_IMPLICIT but no tables in the plan
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM (SELECT CONVERT_IMPLICIT(varchar(10), status) AS status FROM orders) AS subq",
            EstimatedTotalCost = 100,
            Nodes = new List<PlanNode>()
        };

        // Act
        var recommendations = _rule.Evaluate(plan).ToList();

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void Name_ReturnsLowercaseRuleName()
    {
        // Act & Assert
        Assert.Equal("implicit-conversion", _rule.Name);
    }
}
