using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

public class ExecutionPlanExtensionsTests
{
    private readonly ExecutionPlan _emptyPlan = new()
    {
        Dialect = PlanDialect.SqlServer,
        StatementText = "SELECT 1",
        Nodes = new List<PlanNode>(),
        EngineMissingIndexes = new List<EngineMissingIndex>()
    };

    private readonly ExecutionPlan _planWithScans = new()
    {
        Dialect = PlanDialect.SqlServer,
        StatementText = "SELECT * FROM Users WHERE Name = 'test'",
        Nodes = new List<PlanNode>
        {
            new PlanNode
            {
                Operator = "Clustered Index Scan",
                TableName = "dbo.Users",
                IndexName = "PK_Users",
                EstimatedRows = 1000,
                EstimatedRowsRead = 1000,
                RelativeCost = 0.75,
                PredicateColumns = new List<string> { "Name" },
                OutputColumns = new List<string> { "UserId", "Name", "Email" }
            },
            new PlanNode
            {
                Operator = "Clustered Index Scan",
                TableName = "dbo.Orders",
                IndexName = null,
                EstimatedRows = 500,
                EstimatedRowsRead = 500,
                RelativeCost = 0.25,
                PredicateColumns = new List<string> { "OrderDate" },
                OutputColumns = new List<string> { "OrderId", "OrderDate", "UserId", "Amount" }
            },
            new PlanNode
            {
                Operator = "Index Only Scan",
                TableName = "dbo.Products",
                IndexName = "IX_Products_Name",
                EstimatedRows = 100,
                EstimatedRowsRead = 100,
                RelativeCost = 0.05,
                PredicateColumns = new List<string>(),
                OutputColumns = new List<string> { "ProductId", "Name", "Price" }
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

    private readonly ExecutionPlan _planWithNoScans = new()
    {
        Dialect = PlanDialect.SqlServer,
        StatementText = "SELECT 1",
        Nodes = new List<PlanNode>
        {
            new PlanNode
            {
                Operator = "Compute Scalar",
                TableName = null,
                IndexName = null,
                EstimatedRows = 1,
                EstimatedRowsRead = 1,
                RelativeCost = 1.0,
                PredicateColumns = new List<string>(),
                OutputColumns = new List<string> { "Result" }
            }
        },
        EngineMissingIndexes = new List<EngineMissingIndex>()
    };

    [Fact]
    public void GetScanCandidates_WithValidPlan_ReturnsScanNodes()
    {
        // Act
        var scanCandidates = _planWithScans.GetScanCandidates();

        // Assert
        Assert.NotNull(scanCandidates);
        Assert.Equal(2, scanCandidates.Count());
        Assert.All(scanCandidates, node => Assert.True(node.IsScan));
        Assert.DoesNotContain(scanCandidates, node => node.Operator.Contains("Index Only"));
    }

    [Fact]
    public void GetScanCandidates_WithIndexOnlyScans_ReturnsEmpty()
    {
        // Act
        var scanCandidates = _planWithScans.GetScanCandidates();

        // Assert
        Assert.DoesNotContain(scanCandidates, node => node.Operator.Contains("Index Only"));
    }

    [Fact]
    public void GetScanCandidates_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var scanCandidates = _emptyPlan.GetScanCandidates();

        // Assert
        Assert.NotNull(scanCandidates);
        Assert.Empty(scanCandidates);
    }

    [Fact]
    public void GetScanCandidates_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetScanCandidates());
    }

    [Fact]
    public void GetTotalScanCost_WithValidPlan_ReturnsSumOfScanCosts()
    {
        // Act
        var totalCost = _planWithScans.GetTotalScanCost();

        // Assert
        Assert.Equal(1.0, totalCost); // 0.75 + 0.25
    }

    [Fact]
    public void GetTotalScanCost_WithEmptyPlan_ReturnsZero()
    {
        // Act
        var totalCost = _emptyPlan.GetTotalScanCost();

        // Assert
        Assert.Equal(0.0, totalCost);
    }

    [Fact]
    public void GetTotalScanCost_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetTotalScanCost());
    }

    [Fact]
    public void GetScannedTables_WithValidPlan_ReturnsDistinctTableNames()
    {
        // Act
        var scannedTables = _planWithScans.GetScannedTables();

        // Assert
        Assert.NotNull(scannedTables);
        Assert.Equal(2, scannedTables.Count());
        Assert.Contains("dbo.Users", scannedTables, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("dbo.Orders", scannedTables, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("dbo.Products", scannedTables, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetScannedTables_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var scannedTables = _emptyPlan.GetScannedTables();

        // Assert
        Assert.NotNull(scannedTables);
        Assert.Empty(scannedTables);
    }

    [Fact]
    public void GetScannedTables_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetScannedTables());
    }

    [Fact]
    public void GetPredicateColumns_WithValidPlan_ReturnsDistinctPredicateColumns()
    {
        // Act
        var predicateColumns = _planWithScans.GetPredicateColumns();

        // Assert
        Assert.NotNull(predicateColumns);
        Assert.Equal(2, predicateColumns.Count());
        Assert.Contains("Name", predicateColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("OrderDate", predicateColumns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPredicateColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var predicateColumns = _emptyPlan.GetPredicateColumns();

        // Assert
        Assert.NotNull(predicateColumns);
        Assert.Empty(predicateColumns);
    }

    [Fact]
    public void GetPredicateColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetPredicateColumns());
    }

    [Fact]
    public void GetIncludeCandidateColumns_WithValidPlan_ReturnsNonPredicateOutputColumns()
    {
        // Act
        var includeCandidates = _planWithScans.GetIncludeCandidateColumns();

        // Assert
        Assert.NotNull(includeCandidates);
        Assert.Equal(4, includeCandidates.Count()); // Email, OrderId, UserId, Amount (UserId appears in both scans but is distinct)
        Assert.Contains("UserId", includeCandidates, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Email", includeCandidates, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("OrderId", includeCandidates, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Amount", includeCandidates, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("Name", includeCandidates, StringComparer.OrdinalIgnoreCase); // predicate column
        Assert.DoesNotContain("OrderDate", includeCandidates, StringComparer.OrdinalIgnoreCase); // predicate column
    }

    [Fact]
    public void GetIncludeCandidateColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var includeCandidates = _emptyPlan.GetIncludeCandidateColumns();

        // Assert
        Assert.NotNull(includeCandidates);
        Assert.Empty(includeCandidates);
    }

    [Fact]
    public void GetIncludeCandidateColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetIncludeCandidateColumns());
    }

    [Fact]
    public void GetMissingIndexEqualityColumns_WithValidPlan_ReturnsEqualityColumns()
    {
        // Act
        var equalityColumns = _planWithScans.GetMissingIndexEqualityColumns();

        // Assert
        Assert.NotNull(equalityColumns);
        Assert.Equal(2, equalityColumns.Count());
        Assert.Contains("Name", equalityColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Email", equalityColumns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetMissingIndexEqualityColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var equalityColumns = _emptyPlan.GetMissingIndexEqualityColumns();

        // Assert
        Assert.NotNull(equalityColumns);
        Assert.Empty(equalityColumns);
    }

    [Fact]
    public void GetMissingIndexEqualityColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetMissingIndexEqualityColumns());
    }

    [Fact]
    public void GetMissingIndexInequalityColumns_WithValidPlan_ReturnsInequalityColumns()
    {
        // Arrange
        var planWithInequality = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            Nodes = new List<PlanNode>(),
            EngineMissingIndexes = new List<EngineMissingIndex>
            {
                new EngineMissingIndex
                {
                    Table = "dbo.Orders",
                    EqualityColumns = new List<string> { "UserId" },
                    InequalityColumns = new List<string> { "OrderDate" },
                    IncludeColumns = new List<string>()
                }
            }
        };

        // Act
        var inequalityColumns = planWithInequality.GetMissingIndexInequalityColumns();

        // Assert
        Assert.NotNull(inequalityColumns);
        Assert.Single(inequalityColumns);
        Assert.Contains("OrderDate", inequalityColumns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetMissingIndexInequalityColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var inequalityColumns = _emptyPlan.GetMissingIndexInequalityColumns();

        // Assert
        Assert.NotNull(inequalityColumns);
        Assert.Empty(inequalityColumns);
    }

    [Fact]
    public void GetMissingIndexInequalityColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetMissingIndexInequalityColumns());
    }

    [Fact]
    public void GetMissingIndexIncludeColumns_WithValidPlan_ReturnsIncludeColumns()
    {
        // Act
        var includeColumns = _planWithScans.GetMissingIndexIncludeColumns();

        // Assert
        Assert.NotNull(includeColumns);
        Assert.Single(includeColumns);
        Assert.Contains("CreatedDate", includeColumns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetMissingIndexIncludeColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var includeColumns = _emptyPlan.GetMissingIndexIncludeColumns();

        // Assert
        Assert.NotNull(includeColumns);
        Assert.Empty(includeColumns);
    }

    [Fact]
    public void GetMissingIndexIncludeColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetMissingIndexIncludeColumns());
    }

    [Fact]
    public void HasIndexableScans_WithScans_ReturnsTrue()
    {
        // Act
        var hasIndexableScans = _planWithScans.HasIndexableScans();

        // Assert
        Assert.True(hasIndexableScans);
    }

    [Fact]
    public void HasIndexableScans_WithNoScans_ReturnsFalse()
    {
        // Act
        var hasIndexableScans = _planWithNoScans.HasIndexableScans();

        // Assert
        Assert.False(hasIndexableScans);
    }

    [Fact]
    public void HasIndexableScans_WithEmptyPlan_ReturnsFalse()
    {
        // Act
        var hasIndexableScans = _emptyPlan.HasIndexableScans();

        // Assert
        Assert.False(hasIndexableScans);
    }

    [Fact]
    public void HasIndexableScans_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.HasIndexableScans());
    }

    [Fact]
    public void GetHighestCostScan_WithValidPlan_ReturnsHighestCostScan()
    {
        // Act
        var highestCostScan = _planWithScans.GetHighestCostScan();

        // Assert
        Assert.NotNull(highestCostScan);
        Assert.Equal("Clustered Index Scan", highestCostScan!.Operator);
        Assert.Equal("dbo.Users", highestCostScan.TableName);
        Assert.Equal(0.75, highestCostScan.RelativeCost);
    }

    [Fact]
    public void GetHighestCostScan_WithNoScans_ReturnsNull()
    {
        // Act
        var highestCostScan = _planWithNoScans.GetHighestCostScan();

        // Assert
        Assert.Null(highestCostScan);
    }

    [Fact]
    public void GetHighestCostScan_WithEmptyPlan_ReturnsNull()
    {
        // Act
        var highestCostScan = _emptyPlan.GetHighestCostScan();

        // Assert
        Assert.Null(highestCostScan);
    }

    [Fact]
    public void GetHighestCostScan_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetHighestCostScan());
    }

    [Fact]
    public void GetAlreadyCoveredColumns_WithValidPlan_ReturnsColumnsInBothPredicateAndOutput()
    {
        // Arrange - modify plan to have overlapping columns
        var planWithOverlap = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            Nodes = new List<PlanNode>
            {
                new PlanNode
                {
                    Operator = "Clustered Index Scan",
                    TableName = "dbo.Users",
                    IndexName = "PK_Users",
                    EstimatedRows = 1000,
                    EstimatedRowsRead = 1000,
                    RelativeCost = 1.0,
                    PredicateColumns = new List<string> { "UserId", "Name" },
                    OutputColumns = new List<string> { "UserId", "Name", "Email" }
                }
            },
            EngineMissingIndexes = new List<EngineMissingIndex>()
        };

        // Act
        var coveredColumns = planWithOverlap.GetAlreadyCoveredColumns();

        // Assert
        Assert.NotNull(coveredColumns);
        Assert.Equal(2, coveredColumns.Count());
        Assert.Contains("UserId", coveredColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Name", coveredColumns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAlreadyCoveredColumns_WithEmptyPlan_ReturnsEmpty()
    {
        // Act
        var coveredColumns = _emptyPlan.GetAlreadyCoveredColumns();

        // Assert
        Assert.NotNull(coveredColumns);
        Assert.Empty(coveredColumns);
    }

    [Fact]
    public void GetAlreadyCoveredColumns_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.GetAlreadyCoveredColumns());
    }
}