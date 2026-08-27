using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Test class for the ExecutionPlan model.
/// </summary>
public class ExecutionPlanTests : IExecutionPlanTests
{
    private readonly ExecutionPlan _samplePlan = new()
    {
        Dialect = PlanDialect.SqlServer,
        StatementText = "SELECT * FROM Users WHERE Name = 'test'",
        EstimatedTotalCost = 123.45,
        Nodes = new List<PlanNode>
        {
            new PlanNode
            {
                Operator = "Clustered Index Scan",
                TableName = "dbo.Users",
                IndexName = "PK_Users",
                EstimatedRows = 1000,
                EstimatedRowsRead = 1000
            },
            new PlanNode
            {
                Operator = "Clustered Index Scan",
                TableName = "dbo.Orders",
                IndexName = null,
                EstimatedRows = 500,
                EstimatedRowsRead = 500
            }
        },
        EngineMissingIndexes = new List<EngineMissingIndex>
        {
            new EngineMissingIndex
            {
                Table = "dbo.Users",
                ImpactPercent = 95.5,
                EqualityColumns = new List<string> { "Name", "Email" },
                InequalityColumns = new List<string>(),
                IncludeColumns = new List<string> { "CreatedDate" }
            }
        }
    };

    private readonly ExecutionPlan _emptyPlan = new()
    {
        Dialect = PlanDialect.Postgres,
        StatementText = string.Empty,
        EstimatedTotalCost = 0,
        Nodes = new List<PlanNode>(),
        EngineMissingIndexes = new List<EngineMissingIndex>()
    };

    /// <summary>
    /// Tests that Constructor WithDefaultValues CreatesEmptyCollections.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultValues_CreatesEmptyCollections()
    {
        // Act
        var plan = new ExecutionPlan();

        // Assert
        Assert.Equal(PlanDialect.SqlServer, plan.Dialect);
        Assert.Equal(string.Empty, plan.StatementText);
        Assert.Equal(0, plan.EstimatedTotalCost);
        Assert.NotNull(plan.Nodes);
        Assert.Empty(plan.Nodes);
        Assert.NotNull(plan.EngineMissingIndexes);
        Assert.Empty(plan.EngineMissingIndexes);
    }

    /// <summary>
    /// Tests that Dialect SetAndGet ReturnsCorrectValue.
    /// </summary>
    [Fact]
    public void Dialect_SetAndGet_ReturnsCorrectValue()
    {
        // Arrange & Act
        var plan = new ExecutionPlan { Dialect = PlanDialect.Postgres };

        // Assert
        Assert.Equal(PlanDialect.Postgres, plan.Dialect);
    }

    /// <summary>
    /// Tests that Dialect WithSqlServerValue ReturnsSqlServer.
    /// </summary>
    [Fact]
    public void Dialect_WithSqlServerValue_ReturnsSqlServer()
    {
        // Act
        var plan = new ExecutionPlan { Dialect = PlanDialect.SqlServer };

        // Assert
        Assert.Equal(PlanDialect.SqlServer, plan.Dialect);
    }

    /// <summary>
    /// Tests that Dialect WithPostgresValue ReturnsPostgres.
    /// </summary>
    [Fact]
    public void Dialect_WithPostgresValue_ReturnsPostgres()
    {
        // Act
        var plan = new ExecutionPlan { Dialect = PlanDialect.Postgres };

        // Assert
        Assert.Equal(PlanDialect.Postgres, plan.Dialect);
    }

    /// <summary>
    /// Tests that StatementText SetAndGet ReturnsCorrectValue.
    /// </summary>
    [Fact]
    public void StatementText_SetAndGet_ReturnsCorrectValue()
    {
        // Arrange & Act
        var plan = new ExecutionPlan { StatementText = "SELECT * FROM Products WHERE Price > 100" };

        // Assert
        Assert.Equal("SELECT * FROM Products WHERE Price > 100", plan.StatementText);
    }

    /// <summary>
    /// Tests that StatementText WithEmptyString ReturnsEmptyString.
    /// </summary>
    [Fact]
    public void StatementText_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var plan = new ExecutionPlan { StatementText = string.Empty };

        // Assert
        Assert.Equal(string.Empty, plan.StatementText);
    }

    /// <summary>
    /// Tests that StatementText WithNullValue DefaultIsEmptyString.
    /// </summary>
    [Fact]
    public void StatementText_WithNullValue_DefaultIsEmptyString()
    {
        // Arrange & Act
        var plan = new ExecutionPlan();

        // Assert
        Assert.Equal(string.Empty, plan.StatementText);
    }

    /// <summary>
    /// Tests that EstimatedTotalCost SetAndGet ReturnsCorrectValue.
    /// </summary>
    [Fact]
    public void EstimatedTotalCost_SetAndGet_ReturnsCorrectValue()
    {
        // Arrange & Act
        var plan = new ExecutionPlan { EstimatedTotalCost = 999.99 };

        // Assert
        Assert.Equal(999.99, plan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that EstimatedTotalCost WithZeroValue ReturnsZero.
    /// </summary>
    [Fact]
    public void EstimatedTotalCost_WithZeroValue_ReturnsZero()
    {
        // Act
        var plan = new ExecutionPlan { EstimatedTotalCost = 0 };

        // Assert
        Assert.Equal(0, plan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that EstimatedTotalCost WithNegativeValue ReturnsNegativeValue.
    /// </summary>
    [Fact]
    public void EstimatedTotalCost_WithNegativeValue_ReturnsNegativeValue()
    {
        // Act
        var plan = new ExecutionPlan { EstimatedTotalCost = -1.5 };

        // Assert
        Assert.Equal(-1.5, plan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that Nodes SetAndGet ReturnsCorrectCollection.
    /// </summary>
    [Fact]
    public void Nodes_SetAndGet_ReturnsCorrectCollection()
    {
        // Arrange
        var nodes = new List<PlanNode>
        {
            new PlanNode { Operator = "Index Scan" },
            new PlanNode { Operator = "Filter" }
        };

        // Arrange & Act
        var plan = new ExecutionPlan { Nodes = nodes };

        // Assert
        Assert.NotNull(plan.Nodes);
        Assert.Equal(2, plan.Nodes.Count);
        Assert.Equal("Index Scan", plan.Nodes[0].Operator);
        Assert.Equal("Filter", plan.Nodes[1].Operator);
    }

    /// <summary>
    /// Tests that Nodes WithEmptyList ReturnsEmptyCollection.
    /// </summary>
    [Fact]
    public void Nodes_WithEmptyList_ReturnsEmptyCollection()
    {
        // Act
        var plan = new ExecutionPlan { Nodes = new List<PlanNode>() };

        // Assert
        Assert.NotNull(plan.Nodes);
        Assert.Empty(plan.Nodes);
    }

    /// <summary>
    /// Tests that Nodes WithNullValue DefaultIsEmptyList.
    /// </summary>
    [Fact]
    public void Nodes_WithNullValue_DefaultIsEmptyList()
    {
        // Arrange & Act
        var plan = new ExecutionPlan();

        // Assert
        Assert.NotNull(plan.Nodes);
        Assert.Empty(plan.Nodes);
    }

    /// <summary>
    /// Tests that EngineMissingIndexes SetAndGet ReturnsCorrectCollection.
    /// </summary>
    [Fact]
    public void EngineMissingIndexes_SetAndGet_ReturnsCorrectCollection()
    {
        // Arrange
        var missingIndexes = new List<EngineMissingIndex>
        {
            new EngineMissingIndex { Table = "Table1", ImpactPercent = 50 },
            new EngineMissingIndex { Table = "Table2", ImpactPercent = 75 }
        };

        // Arrange & Act
        var plan = new ExecutionPlan { EngineMissingIndexes = missingIndexes };

        // Assert
        Assert.NotNull(plan.EngineMissingIndexes);
        Assert.Equal(2, plan.EngineMissingIndexes.Count);
        Assert.Equal("Table1", plan.EngineMissingIndexes[0].Table);
        Assert.Equal("Table2", plan.EngineMissingIndexes[1].Table);
    }

    /// <summary>
    /// Tests that EngineMissingIndexes WithEmptyList ReturnsEmptyCollection.
    /// </summary>
    [Fact]
    public void EngineMissingIndexes_WithEmptyList_ReturnsEmptyCollection()
    {
        // Act
        var plan = new ExecutionPlan { EngineMissingIndexes = new List<EngineMissingIndex>() };

        // Assert
        Assert.NotNull(plan.EngineMissingIndexes);
        Assert.Empty(plan.EngineMissingIndexes);
    }

    /// <summary>
    /// Tests that EngineMissingIndexes WithNullValue DefaultIsEmptyList.
    /// </summary>
    [Fact]
    public void EngineMissingIndexes_WithNullValue_DefaultIsEmptyList()
    {
        // Arrange & Act
        var plan = new ExecutionPlan();

        // Assert
        Assert.NotNull(plan.EngineMissingIndexes);
        Assert.Empty(plan.EngineMissingIndexes);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectDialect.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectDialect()
    {
        // Assert
        Assert.Equal(PlanDialect.SqlServer, _samplePlan.Dialect);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectStatementText.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectStatementText()
    {
        // Assert
        Assert.Equal("SELECT * FROM Users WHERE Name = 'test'", _samplePlan.StatementText);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectEstimatedTotalCost.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectEstimatedTotalCost()
    {
        // Assert
        Assert.Equal(123.45, _samplePlan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectNodesCount.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectNodesCount()
    {
        // Assert
        Assert.Equal(2, _samplePlan.Nodes.Count);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectEngineMissingIndexesCount.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectEngineMissingIndexesCount()
    {
        // Assert
        Assert.Equal(1, _samplePlan.EngineMissingIndexes.Count);
    }

    /// <summary>
    /// Tests that EmptyPlan HasCorrectDialect.
    /// </summary>
    [Fact]
    public void EmptyPlan_HasCorrectDialect()
    {
        // Assert
        Assert.Equal(PlanDialect.Postgres, _emptyPlan.Dialect);
    }

    /// <summary>
    /// Tests that EmptyPlan HasEmptyStatementText.
    /// </summary>
    [Fact]
    public void EmptyPlan_HasEmptyStatementText()
    {
        // Assert
        Assert.Equal(string.Empty, _emptyPlan.StatementText);
    }

    /// <summary>
    /// Tests that EmptyPlan HasZeroEstimatedTotalCost.
    /// </summary>
    [Fact]
    public void EmptyPlan_HasZeroEstimatedTotalCost()
    {
        // Assert
        Assert.Equal(0, _emptyPlan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that EmptyPlan HasEmptyNodes.
    /// </summary>
    [Fact]
    public void EmptyPlan_HasEmptyNodes()
    {
        // Assert
        Assert.Empty(_emptyPlan.Nodes);
    }

    /// <summary>
    /// Tests that EmptyPlan HasEmptyEngineMissingIndexes.
    /// </summary>
    [Fact]
    public void EmptyPlan_HasEmptyEngineMissingIndexes()
    {
        // Assert
        Assert.Empty(_emptyPlan.EngineMissingIndexes);
    }

    /// <summary>
    /// Tests that PlanNode WithParentProperty LinksCorrectly.
    /// </summary>
    [Fact]
    public void PlanNode_WithParentProperty_LinksCorrectly()
    {
        // Arrange
        var parentNode = new PlanNode { Operator = "Parent" };
        var childNode = new PlanNode { Operator = "Child", Parent = parentNode };

        // Assert
        Assert.NotNull(childNode.Parent);
        Assert.Equal("Parent", childNode.Parent.Operator);
        Assert.Null(parentNode.Parent); // parentNode has no parent
    }

    /// <summary>
    /// Tests that PlanNode IsScan WithScanOperator ReturnsTrue.
    /// </summary>
    [Fact]
    public void PlanNode_IsScan_WithScanOperator_ReturnsTrue()
    {
        // Arrange
        var scanNode = new PlanNode { Operator = "Clustered Index Scan" };

        // Assert
        Assert.True(scanNode.IsScan);
    }

    /// <summary>
    /// Tests that PlanNode IsScan WithIndexOnlyScanOperator ReturnsFalse.
    /// </summary>
    [Fact]
    public void PlanNode_IsScan_WithIndexOnlyScanOperator_ReturnsFalse()
    {
        // Arrange
        var indexOnlyNode = new PlanNode { Operator = "Index Only Scan" };

        // Assert
        Assert.False(indexOnlyNode.IsScan);
    }

    /// <summary>
    /// Tests that PlanNode IsScan WithNonScanOperator ReturnsFalse.
    /// </summary>
    [Fact]
    public void PlanNode_IsScan_WithNonScanOperator_ReturnsFalse()
    {
        // Arrange
        var filterNode = new PlanNode { Operator = "Filter" };

        // Assert
        Assert.False(filterNode.IsScan);
    }

    /// <summary>
    /// Tests that EngineMissingIndex Properties SetAndGetCorrectly.
    /// </summary>
    [Fact]
    public void EngineMissingIndex_Properties_SetAndGetCorrectly()
    {
        // Arrange & Act
        var missingIndex = new EngineMissingIndex
        {
            Table = "dbo.TestTable",
            ImpactPercent = 87.3,
            EqualityColumns = new List<string> { "Col1", "Col2" },
            InequalityColumns = new List<string> { "Col3" },
            IncludeColumns = new List<string> { "Col4", "Col5", "Col6" }
        };

        // Assert
        Assert.Equal("dbo.TestTable", missingIndex.Table);
        Assert.Equal(87.3, missingIndex.ImpactPercent);
        Assert.Equal(2, missingIndex.EqualityColumns.Count);
        Assert.Equal("Col1", missingIndex.EqualityColumns[0]);
        Assert.Equal("Col3", missingIndex.InequalityColumns[0]);
        Assert.Equal(3, missingIndex.IncludeColumns.Count);
    }
}
