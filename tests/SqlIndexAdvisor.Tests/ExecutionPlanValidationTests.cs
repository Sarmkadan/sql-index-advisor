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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.SeqScanRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.CountryColumn, ExecutionPlanValidationTestsConstants.IsActiveColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.EmailColumn, ExecutionPlanValidationTestsConstants.CountryColumn }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.SeqScanRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.CountryColumn, ExecutionPlanValidationTestsConstants.IsActiveColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.EmailColumn, ExecutionPlanValidationTestsConstants.CountryColumn }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.SeqScanRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.CountryColumn, ExecutionPlanValidationTestsConstants.IsActiveColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.EmailColumn, ExecutionPlanValidationTestsConstants.CountryColumn }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.NegativeEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.SeqScanEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.SeqScanRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.CountryColumn, ExecutionPlanValidationTestsConstants.IsActiveColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.EmailColumn, ExecutionPlanValidationTestsConstants.CountryColumn }
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
            Dialect = (PlanDialect)ExecutionPlanValidationTestsConstants.InvalidDialectValue,
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.InvalidDialectMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = double.NaN,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.EstimatedTotalCostNaNMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = double.PositiveInfinity,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.EstimatedTotalCostInfiniteMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = null
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NodesNullMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { null }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NullNodeMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = "" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.EmptyOperatorMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator, EstimatedRows = ExecutionPlanValidationTestsConstants.NegativeEstimatedRows } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NegativeEstimatedRowsMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator, EstimatedRows = double.NaN } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NaNEstimatedRowsMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator, RelativeCost = ExecutionPlanValidationTestsConstants.OutOfRangeRelativeCost } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.OutOfRangeRelativeCostMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator, PredicateColumns = null } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NullPredicateColumnsMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator, PredicateColumns = { } }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            EngineMissingIndexes = null
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NullEngineMissingIndexesMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            EngineMissingIndexes = { null }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.NullMissingIndexMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            EngineMissingIndexes = { new EngineMissingIndex { Table = "" } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.EmptyMissingIndexTableMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            EngineMissingIndexes = { new EngineMissingIndex { Table = ExecutionPlanValidationTestsConstants.UsersTable, ImpactPercent = ExecutionPlanValidationTestsConstants.OutOfRangeImpactPercent } }
        };

        // Act
        var problems = plan.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains(ExecutionPlanValidationTestsConstants.OutOfRangeImpactPercentMessage, problems[0]);
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
            StatementText = ExecutionPlanValidationTestsConstants.PostgresActiveUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.PostgresEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.PostgresEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.PostgresEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.PostgresRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.ActiveColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.NameColumn, ExecutionPlanValidationTestsConstants.EmailColumn }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.NegativeEstimatedTotalCostAlt,
            Nodes = { new PlanNode { Operator = ExecutionPlanValidationTestsConstants.SeqScanOperator } }
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
            StatementText = ExecutionPlanValidationTestsConstants.SelectAllUsersStatement,
            EstimatedTotalCost = ExecutionPlanValidationTestsConstants.ValidEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = ExecutionPlanValidationTestsConstants.IndexScanOperator,
                    TableName = ExecutionPlanValidationTestsConstants.UsersTable,
                    EstimatedRows = ExecutionPlanValidationTestsConstants.IndexScanEstimatedRows,
                    EstimatedRowsRead = ExecutionPlanValidationTestsConstants.IndexScanEstimatedRowsRead,
                    RelativeCost = ExecutionPlanValidationTestsConstants.IndexScanRelativeCost,
                    PredicateColumns = { ExecutionPlanValidationTestsConstants.IdColumn },
                    OutputColumns = { ExecutionPlanValidationTestsConstants.IdColumn, ExecutionPlanValidationTestsConstants.NameColumn }
                }
            }
        };

        // Act
        var act = () => plan.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }
}
