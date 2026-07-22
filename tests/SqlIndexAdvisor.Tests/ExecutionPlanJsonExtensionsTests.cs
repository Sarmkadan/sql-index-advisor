using System;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

public class ExecutionPlanJsonExtensionsTests
{
    private readonly ExecutionPlan _samplePlan = new()
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
        Nodes = new List<PlanNode>(),
        EngineMissingIndexes = new List<EngineMissingIndex>()
    };

    [Fact]
    public void ToJson_WithValidPlan_ReturnsNonEmptyJsonString()
    {
        // Act
        var json = _samplePlan.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_WithValidPlan_ContainsExpectedProperties()
    {
        // Act
        var json = _samplePlan.ToJson();

        // Assert
        Assert.Contains("\"dialect\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"statementText\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"nodes\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"engineMissingIndexes\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Act
        var json = _samplePlan.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.StartsWith("{\n", json);
        Assert.Contains("\n", json); // Should have newlines for formatting
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Act
        var json = _samplePlan.ToJson(indented: false);

        // Assert
        Assert.NotNull(json);
        Assert.DoesNotContain("\n", json); // Should not have newlines
    }

    [Fact]
    public void ToJson_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        ExecutionPlan? nullPlan = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullPlan!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsExecutionPlan()
    {
        // Arrange
        var json = _samplePlan.ToJson();

        // Act
        var result = ExecutionPlanJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PlanDialect.SqlServer, result.Dialect);
        Assert.Equal("SELECT * FROM Users WHERE Name = 'test'", result.StatementText);
        Assert.Equal(2, result.Nodes.Count);
        Assert.Equal(1, result.EngineMissingIndexes.Count);
    }

    [Fact]
    public void FromJson_WithEmptyPlanJson_ReturnsEmptyPlan()
    {
        // Arrange
        var json = _emptyPlan.ToJson();

        // Act
        var result = ExecutionPlanJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PlanDialect.Postgres, result.Dialect);
        Assert.Empty(result.Nodes);
        Assert.Empty(result.EngineMissingIndexes);
    }

    [Fact]
    public void FromJson_WithCamelCaseProperties_ReturnsCorrectlyDeserializedPlan()
    {
        // Arrange
        var json = _samplePlan.ToJson();

        // Act
        var result = ExecutionPlanJsonExtensions.FromJson(json);

        // Assert - verify camelCase properties are correctly deserialized to PascalCase
        Assert.Equal(PlanDialect.SqlServer, result.Dialect);
        Assert.Equal("SELECT * FROM Users WHERE Name = 'test'", result.StatementText);
        Assert.Equal(2, result.Nodes.Count);
        Assert.Equal("Clustered Index Scan", result.Nodes[0].Operator);
        Assert.Equal("dbo.Users", result.Nodes[0].TableName);
        Assert.Equal(0.75, result.Nodes[0].RelativeCost);
        Assert.Single(result.Nodes[0].PredicateColumns);
        Assert.Equal("Name", result.Nodes[0].PredicateColumns[0]);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ExecutionPlanJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExecutionPlanJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsJsonException()
    {
        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => ExecutionPlanJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => ExecutionPlanJsonExtensions.FromJson("{ invalid json }"));
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = _samplePlan.ToJson();

        // Act
        var result = ExecutionPlanJsonExtensions.TryFromJson(json, out var deserializedPlan);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedPlan);
        Assert.Equal(PlanDialect.SqlServer, deserializedPlan.Dialect);
        Assert.Equal(2, deserializedPlan.Nodes.Count);
    }

    [Fact]
    public void TryFromJson_WithEmptyPlanJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = _emptyPlan.ToJson();

        // Act
        var result = ExecutionPlanJsonExtensions.TryFromJson(json, out var deserializedPlan);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedPlan);
        Assert.Empty(deserializedPlan.Nodes);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ExecutionPlanJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExecutionPlanJsonExtensions.TryFromJson(string.Empty, out _));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalse()
    {
        // Act
        var result = ExecutionPlanJsonExtensions.TryFromJson("   ", out var deserializedPlan);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedPlan);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = ExecutionPlanJsonExtensions.TryFromJson(invalidJson, out var deserializedPlan);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedPlan);
    }

    [Fact]
    public void RoundTripSerialization_WithSamplePlan_ReturnsEquivalentPlan()
    {
        // Arrange
        var originalPlan = _samplePlan;

        // Act
        var json = originalPlan.ToJson();
        var deserializedPlan = ExecutionPlanJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedPlan);
        Assert.Equal(originalPlan.Dialect, deserializedPlan.Dialect);
        Assert.Equal(originalPlan.StatementText, deserializedPlan.StatementText);
        Assert.Equal(originalPlan.Nodes.Count, deserializedPlan.Nodes.Count);
        Assert.Equal(originalPlan.EngineMissingIndexes.Count, deserializedPlan.EngineMissingIndexes.Count);

        // Verify nodes match
        for (int i = 0; i < originalPlan.Nodes.Count; i++)
        {
            Assert.Equal(originalPlan.Nodes[i].Operator, deserializedPlan.Nodes[i].Operator);
            Assert.Equal(originalPlan.Nodes[i].TableName, deserializedPlan.Nodes[i].TableName);
            Assert.Equal(originalPlan.Nodes[i].IndexName, deserializedPlan.Nodes[i].IndexName);
            Assert.Equal(originalPlan.Nodes[i].RelativeCost, deserializedPlan.Nodes[i].RelativeCost);
            Assert.Equal(originalPlan.Nodes[i].PredicateColumns.Count, deserializedPlan.Nodes[i].PredicateColumns.Count);
            Assert.Equal(originalPlan.Nodes[i].OutputColumns.Count, deserializedPlan.Nodes[i].OutputColumns.Count);
        }

        // Verify missing indexes match
        for (int i = 0; i < originalPlan.EngineMissingIndexes.Count; i++)
        {
            Assert.Equal(originalPlan.EngineMissingIndexes[i].Table, deserializedPlan.EngineMissingIndexes[i].Table);
            Assert.Equal(originalPlan.EngineMissingIndexes[i].ImpactPercent, deserializedPlan.EngineMissingIndexes[i].ImpactPercent);
            Assert.Equal(originalPlan.EngineMissingIndexes[i].EqualityColumns.Count, deserializedPlan.EngineMissingIndexes[i].EqualityColumns.Count);
            Assert.Equal(originalPlan.EngineMissingIndexes[i].InequalityColumns.Count, deserializedPlan.EngineMissingIndexes[i].InequalityColumns.Count);
            Assert.Equal(originalPlan.EngineMissingIndexes[i].IncludeColumns.Count, deserializedPlan.EngineMissingIndexes[i].IncludeColumns.Count);
        }
    }

    [Fact]
    public void RoundTripSerialization_WithEmptyPlan_ReturnsEquivalentPlan()
    {
        // Arrange
        var originalPlan = _emptyPlan;

        // Act
        var json = originalPlan.ToJson();
        var deserializedPlan = ExecutionPlanJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedPlan);
        Assert.Equal(originalPlan.Dialect, deserializedPlan.Dialect);
        Assert.Empty(deserializedPlan.Nodes);
        Assert.Empty(deserializedPlan.EngineMissingIndexes);
    }
}