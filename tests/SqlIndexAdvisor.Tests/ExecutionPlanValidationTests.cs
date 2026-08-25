using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides unit tests for validating <see cref="ExecutionPlan"/> instances using the <see cref="IExecutionPlanValidationTests"/> interface.
/// </summary>
public class ExecutionPlanValidationTests : IExecutionPlanValidationTests
{
    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.Validate"/> on a valid execution plan returns no validation problems.
    /// </summary>
    [Fact]
    public void Validate_HappyPath_ForEachMajorPublicMethod_ReturnsNoProblems()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 50,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "country", "is_active" },
                    OutputColumns = { "id", "email", "country" }
                }
            }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.Validate"/> on a null execution plan throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan plan = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => plan.Validate());
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.IsValid"/> on a valid execution plan returns true.
    /// </summary>
    [Fact]
    public void IsValid_HappyPath_ForEachMajorPublicMethod_ReturnsTrue()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 50,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "country", "is_active" },
                    OutputColumns = { "id", "email", "country" }
                }
            }
        };

        // Act
        var isValid = plan.IsValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.IsValid"/> on a null execution plan throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan plan = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => plan.IsValid());
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.EnsureValid"/> on a valid execution plan does not throw an exception.
    /// </summary>
    [Fact]
    public void EnsureValid_HappyPath_ForEachMajorPublicMethod_DoesNotThrow()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 50,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "country", "is_active" },
                    OutputColumns = { "id", "email", "country" }
                }
            }
        };

        // Act
        var act = () => plan.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.EnsureValid"/> on a null execution plan throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan plan = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => plan.EnsureValid());
    }

    /// <summary>
    /// Verifies that calling <see cref="ExecutionPlan.EnsureValid"/> on an invalid execution plan (e.g., negative cost) throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidPlan_ThrowsArgumentException()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = -100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 50,
                    EstimatedRowsRead = 500000,
                    RelativeCost = 0.9,
                    PredicateColumns = { "country", "is_active" },
                    OutputColumns = { "id", "email", "country" }
                }
            }
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => plan.EnsureValid());
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when the plan's <see cref="ExecutionPlan.Dialect"/> is an invalid enum value.
    /// </summary>
    [Fact]
    public void Validate_InvalidDialect_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = (PlanDialect)999,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Invalid Dialect value", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when <see cref="ExecutionPlan.EstimatedTotalCost"/> is NaN.
    /// </summary>
    [Fact]
    public void Validate_NaNEstimatedTotalCost_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = double.NaN,
            Nodes = { new PlanNode { Operator = "Seq Scan" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EstimatedTotalCost cannot be NaN", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when <see cref="ExecutionPlan.EstimatedTotalCost"/> is infinite.
    /// </summary>
    [Fact]
    public void Validate_InfiniteEstimatedTotalCost_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = double.PositiveInfinity,
            Nodes = { new PlanNode { Operator = "Seq Scan" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EstimatedTotalCost cannot be infinite", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when the <see cref="ExecutionPlan.Nodes"/> collection is null.
    /// </summary>
    [Fact]
    public void Validate_NullNodesCollection_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = null
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes collection cannot be null", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node within the <see cref="ExecutionPlan.Nodes"/> collection is null.
    /// </summary>
    [Fact]
    public void Validate_NullNodeInCollection_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { null }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0] cannot be null", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node's <see cref="PlanNode.Operator"/> is an empty string.
    /// </summary>
    [Fact]
    public void Validate_EmptyOperator_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0].Operator cannot be null or empty", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node's <see cref="PlanNode.EstimatedRows"/> is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeEstimatedRows_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", EstimatedRows = -1 } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0].EstimatedRows cannot be negative", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node's <see cref="PlanNode.EstimatedRows"/> is NaN.
    /// </summary>
    [Fact]
    public void Validate_NaNEstimatedRows_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", EstimatedRows = double.NaN } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0].EstimatedRows cannot be NaN", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node's <see cref="PlanNode.RelativeCost"/> is outside the valid 0 to 1 range.
    /// </summary>
    [Fact]
    public void Validate_OutOfRangeRelativeCost_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", RelativeCost = 1.5 } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0].RelativeCost must be between 0 and 1", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a node's <see cref="PlanNode.PredicateColumns"/> collection is null.
    /// </summary>
    [Fact]
    public void Validate_NullPredicateColumnsCollection_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", PredicateColumns = null } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Nodes[0].PredicateColumns cannot be null", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> does not return a validation problem when a node's <see cref="PlanNode.PredicateColumns"/> collection is empty.
    /// </summary>
    [Fact]
    public void Validate_EmptyPredicateColumns_ReturnsNoProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", PredicateColumns = { } }
            }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when the <see cref="ExecutionPlan.EngineMissingIndexes"/> collection is null.
    /// </summary>
    [Fact]
    public void Validate_NullEngineMissingIndexesCollection_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            EngineMissingIndexes = null
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EngineMissingIndexes collection cannot be null", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when an item within the <see cref="ExecutionPlan.EngineMissingIndexes"/> collection is null.
    /// </summary>
    [Fact]
    public void Validate_NullMissingIndex_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            EngineMissingIndexes = { null }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EngineMissingIndexes[0] cannot be null", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a missing index entry has an empty <see cref="EngineMissingIndex.Table"/> name.
    /// </summary>
    [Fact]
    public void Validate_EmptyTableNameInMissingIndex_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            EngineMissingIndexes = { new EngineMissingIndex { Table = "" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EngineMissingIndexes[0].Table cannot be null or empty", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.Validate"/> returns a validation problem when a missing index entry's <see cref="EngineMissingIndex.ImpactPercent"/> is outside the valid 0 to 100 range.
    /// </summary>
    [Fact]
    public void Validate_OutOfRangeImpactPercent_ReturnsProblem()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            EngineMissingIndexes = { new EngineMissingIndex { Table = "users", ImpactPercent = 150 } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("EngineMissingIndexes[0].ImpactPercent must be between 0 and 100", problems[0]);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.IsValid"/> returns true when evaluated against a valid execution plan.
    /// </summary>
    [Fact]
    public void Validate_IsValid_WithValidPlan_ReturnsTrue()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            StatementText = "SELECT * FROM users WHERE active = true",
            EstimatedTotalCost = 45.5,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "users",
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000,
                    RelativeCost = 0.85,
                    PredicateColumns = { "active" },
                    OutputColumns = { "id", "name", "email" }
                }
            }
        };

        // Act
        var isValid = plan.IsValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.IsValid"/> returns false when evaluated against an invalid execution plan.
    /// </summary>
    [Fact]
    public void Validate_IsValid_WithInvalidPlan_ReturnsFalse()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = -50,
            Nodes = { new PlanNode { Operator = "Seq Scan" } }
        };

        // Act
        var isValid = plan.IsValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that <see cref="ExecutionPlan.EnsureValid"/> does not throw an exception when evaluated against a valid execution plan.
    /// </summary>
    [Fact]
    public void Validate_EnsureValid_WithValidPlan_DoesNotThrow()
    {
        // Arrange
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = "SELECT * FROM users",
            EstimatedTotalCost = 100,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Index Scan",
                    TableName = "users",
                    EstimatedRows = 10,
                    EstimatedRowsRead = 10,
                    RelativeCost = 0.1,
                    PredicateColumns = { "id" },
                    OutputColumns = { "id", "name" }
                }
            }
        };

        // Act
        var act = () => plan.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }
}
