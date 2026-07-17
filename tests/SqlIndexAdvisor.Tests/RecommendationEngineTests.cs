using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;
using Xunit;

/// <summary>
/// Tests for the RecommendationEngine class.
/// </summary>
namespace SqlIndexAdvisor.Tests;

public class RecommendationEngineTests
{
    /// <summary>
    /// Verifies that a Seq Scan with a filter produces a recommendation.
    /// </summary>
    [Fact]
    public void SeqScanWithFilterProducesRecommendation()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
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

        var rec = Assert.Single(new RecommendationEngine().Analyze(plan));
        Assert.Equal("users", rec.Table);
        Assert.Equal(new[] { "country", "is_active" }, rec.KeyColumns);
        // id and email are not predicate columns -> INCLUDE; country is a key so excluded.
        Assert.Contains("id", rec.IncludeColumns);
        Assert.Contains("email", rec.IncludeColumns);
        Assert.DoesNotContain("country", rec.IncludeColumns);
        Assert.Equal(Confidence.High, rec.Confidence);
    }

    /// <summary>
    /// Verifies that a cheap scan is ignored.
    /// </summary>
    [Fact]
    public void CheapScanIsIgnored()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Seq Scan",
                    TableName = "tiny",
                    RelativeCost = 0.02,
                    PredicateColumns = { "x" }
                }
            }
        };
        Assert.Empty(new RecommendationEngine().Analyze(plan));
    }

    /// <summary>
    /// Verifies that engine hints and scans on the same keys are merged.
    /// </summary>
    [Fact]
    public void EngineHintAndScanOnSameKeysAreMerged()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 10,
            EngineMissingIndexes =
            {
                new EngineMissingIndex
                {
                    Table = "dbo.Orders",
                    ImpactPercent = 80,
                    EqualityColumns = { "Status" },
                    IncludeColumns = { "Total" }
                }
            },
            Nodes =
            {
                new PlanNode
                {
                    Operator = "Clustered Index Scan",
                    TableName = "dbo.Orders",
                    RelativeCost = 0.95,
                    EstimatedRows = 10,
                    EstimatedRowsRead = 1000,
                    PredicateColumns = { "Status" },
                    OutputColumns = { "Total", "CustomerId" }
                }
            }
        };

        var recs = new RecommendationEngine().Analyze(plan);
        var rec = Assert.Single(recs);
        Assert.Equal("dbo.Orders", rec.Table);
        Assert.Equal(new[] { "Status" }, rec.KeyColumns);
        // includes merged from both sources
        Assert.Contains("Total", rec.IncludeColumns);
        Assert.Contains("CustomerId", rec.IncludeColumns);
        Assert.Equal(Confidence.High, rec.Confidence);
        Assert.True(rec.Reasons.Count >= 2);
    }

    /// <summary>
    /// Verifies that the create statement includes keys and includes.
    /// </summary>
    [Fact]
    public void CreateStatementIncludesKeysAndIncludes()
    {
        var rec = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new() { "Status", "CreatedAt" },
            IncludeColumns = { "Total" }
        };
        var sql = rec.ToCreateStatement(PlanDialect.SqlServer);
        Assert.Contains("CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt)", sql);
        Assert.Contains("INCLUDE (Total)", sql);
    }
}
