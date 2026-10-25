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
        StatementText = ExecutionPlanTestsConstants.UsersByNameStatement,
        EstimatedTotalCost = ExecutionPlanTestsConstants.SamplePlanEstimatedTotalCost,
        Nodes = new List<PlanNode>
        {
            new PlanNode
            {
                Operator = ExecutionPlanTestsConstants.ClusteredIndexScanOperator,
                TableName = ExecutionPlanTestsConstants.UsersTable,
                IndexName = ExecutionPlanTestsConstants.PrimaryKeyUsersIndex,
                EstimatedRows = ExecutionPlanTestsConstants.SamplePlanUsersEstimatedRows,
                EstimatedRowsRead = ExecutionPlanTestsConstants.SamplePlanUsersEstimatedRows
            },
            new PlanNode
            {
                Operator = ExecutionPlanTestsConstants.ClusteredIndexScanOperator,
                TableName = ExecutionPlanTestsConstants.OrdersTable,
                IndexName = null,
                EstimatedRows = ExecutionPlanTestsConstants.SamplePlanOrdersEstimatedRows,
                EstimatedRowsRead = ExecutionPlanTestsConstants.SamplePlanOrdersEstimatedRows
            }
        },
        EngineMissingIndexes = new List<EngineMissingIndex>
        {
            new EngineMissingIndex
            {
                Table = ExecutionPlanTestsConstants.UsersTable,
                ImpactPercent = ExecutionPlanTestsConstants.SamplePlanImpactPercent,
                EqualityColumns = new List<string> { ExecutionPlanTestsConstants.NameColumn, ExecutionPlanTestsConstants.EmailColumn },
                InequalityColumns = new List<string>(),
                IncludeColumns = new List<string> { ExecutionPlanTestsConstants.CreatedDateColumn }
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
        var plan = new ExecutionPlan { StatementText = ExecutionPlanTestsConstants.ProductsByPriceStatement };

        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.ProductsByPriceStatement, plan.StatementText);
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
        var plan = new ExecutionPlan { EstimatedTotalCost = ExecutionPlanTestsConstants.MaxEstimatedTotalCost };

        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.MaxEstimatedTotalCost, plan.EstimatedTotalCost);
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
        var plan = new ExecutionPlan { EstimatedTotalCost = ExecutionPlanTestsConstants.NegativeEstimatedTotalCost };

        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.NegativeEstimatedTotalCost, plan.EstimatedTotalCost);
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
            new PlanNode { Operator = ExecutionPlanTestsConstants.IndexScanOperator },
            new PlanNode { Operator = ExecutionPlanTestsConstants.FilterOperator }
        };

        // Arrange & Act
        var plan = new ExecutionPlan { Nodes = nodes };

        // Assert
        Assert.NotNull(plan.Nodes);
        Assert.Equal(ExecutionPlanTestsConstants.TwoItemCollectionCount, plan.Nodes.Count);
        Assert.Equal(ExecutionPlanTestsConstants.IndexScanOperator, plan.Nodes[0].Operator);
        Assert.Equal(ExecutionPlanTestsConstants.FilterOperator, plan.Nodes[1].Operator);
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
            new EngineMissingIndex { Table = ExecutionPlanTestsConstants.Table1, ImpactPercent = ExecutionPlanTestsConstants.MissingIndexImpactPercent },
            new EngineMissingIndex { Table = ExecutionPlanTestsConstants.Table2, ImpactPercent = ExecutionPlanTestsConstants.OtherMissingIndexImpactPercent }
        };

        // Arrange & Act
        var plan = new ExecutionPlan { EngineMissingIndexes = missingIndexes };

        // Assert
        Assert.NotNull(plan.EngineMissingIndexes);
        Assert.Equal(ExecutionPlanTestsConstants.TwoItemCollectionCount, plan.EngineMissingIndexes.Count);
        Assert.Equal(ExecutionPlanTestsConstants.Table1, plan.EngineMissingIndexes[0].Table);
        Assert.Equal(ExecutionPlanTestsConstants.Table2, plan.EngineMissingIndexes[1].Table);
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
        Assert.Equal(ExecutionPlanTestsConstants.UsersByNameStatement, _samplePlan.StatementText);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectEstimatedTotalCost.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectEstimatedTotalCost()
    {
        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.SamplePlanEstimatedTotalCost, _samplePlan.EstimatedTotalCost);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectNodesCount.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectNodesCount()
    {
        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.SamplePlanNodeCount, _samplePlan.Nodes.Count);
    }

    /// <summary>
    /// Tests that SamplePlan HasCorrectEngineMissingIndexesCount.
    /// </summary>
    [Fact]
    public void SamplePlan_HasCorrectEngineMissingIndexesCount()
    {
        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.SamplePlanEngineMissingIndexCount, _samplePlan.EngineMissingIndexes.Count);
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
        var parentNode = new PlanNode { Operator = ExecutionPlanTestsConstants.ParentOperator };
        var childNode = new PlanNode { Operator = ExecutionPlanTestsConstants.ChildOperator, Parent = parentNode };

        // Assert
        Assert.NotNull(childNode.Parent);
        Assert.Equal(ExecutionPlanTestsConstants.ParentOperator, childNode.Parent.Operator);
        Assert.Null(parentNode.Parent); // parentNode has no parent
    }

    /// <summary>
    /// Tests that PlanNode IsScan WithScanOperator ReturnsTrue.
    /// </summary>
    [Fact]
    public void PlanNode_IsScan_WithScanOperator_ReturnsTrue()
    {
        // Arrange
        var scanNode = new PlanNode { Operator = ExecutionPlanTestsConstants.ClusteredIndexScanOperator };

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
        var indexOnlyNode = new PlanNode { Operator = ExecutionPlanTestsConstants.IndexOnlyScanOperator };

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
        var filterNode = new PlanNode { Operator = ExecutionPlanTestsConstants.FilterOperator };

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
            Table = ExecutionPlanTestsConstants.TestTable,
            ImpactPercent = ExecutionPlanTestsConstants.TestTableImpactPercent,
            EqualityColumns = new List<string> { ExecutionPlanTestsConstants.Col1Column, ExecutionPlanTestsConstants.Col2Column },
            InequalityColumns = new List<string> { ExecutionPlanTestsConstants.Col3Column },
            IncludeColumns = new List<string> { ExecutionPlanTestsConstants.Col4Column, ExecutionPlanTestsConstants.Col5Column, ExecutionPlanTestsConstants.Col6Column }
        };

        // Assert
        Assert.Equal(ExecutionPlanTestsConstants.TestTable, missingIndex.Table);
        Assert.Equal(ExecutionPlanTestsConstants.TestTableImpactPercent, missingIndex.ImpactPercent);
        Assert.Equal(ExecutionPlanTestsConstants.TestTableEqualityColumnCount, missingIndex.EqualityColumns.Count);
        Assert.Equal(ExecutionPlanTestsConstants.Col1Column, missingIndex.EqualityColumns[0]);
        Assert.Equal(ExecutionPlanTestsConstants.Col3Column, missingIndex.InequalityColumns[0]);
        Assert.Equal(ExecutionPlanTestsConstants.TestTableIncludeColumnCount, missingIndex.IncludeColumns.Count);
    }
}
