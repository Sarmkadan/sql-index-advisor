using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;
using Xunit;

/// <summary>
/// Tests for the RecommendationEngine class.
/// </summary>
namespace SqlIndexAdvisor.Tests;

public class RecommendationEngineTests : IRecommendationEngineTests
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
            EstimatedTotalCost = RecommendationEngineTestsConstants.DefaultEstimatedTotalCost,
            Nodes =
            {
                new PlanNode
                {
                    Operator = RecommendationEngineTestsConstants.SeqScanOperator,
                    TableName = RecommendationEngineTestsConstants.UsersTable,
                    EstimatedRows = RecommendationEngineTestsConstants.DefaultEstimatedRows,
                    EstimatedRowsRead = RecommendationEngineTestsConstants.DefaultEstimatedRowsRead,
                    RelativeCost = RecommendationEngineTestsConstants.DefaultRelativeCost,
                    PredicateColumns = { RecommendationEngineTestsConstants.CountryColumn, RecommendationEngineTestsConstants.IsActiveColumn },
                    OutputColumns = { RecommendationEngineTestsConstants.IdColumn, RecommendationEngineTestsConstants.EmailColumn, RecommendationEngineTestsConstants.CountryColumn }
                }
            }
        };

        var rec = Assert.Single(new RecommendationEngine().Analyze(plan));
        Assert.Equal(RecommendationEngineTestsConstants.UsersTable, rec.Table);
        Assert.Equal(new[] { RecommendationEngineTestsConstants.CountryColumn, RecommendationEngineTestsConstants.IsActiveColumn }, rec.KeyColumns);
        // id and email are not predicate columns -> INCLUDE; country is a key so excluded.
        Assert.Contains(RecommendationEngineTestsConstants.IdColumn, rec.IncludeColumns);
        Assert.Contains(RecommendationEngineTestsConstants.EmailColumn, rec.IncludeColumns);
        Assert.DoesNotContain(RecommendationEngineTestsConstants.CountryColumn, rec.IncludeColumns);
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
                    Operator = RecommendationEngineTestsConstants.SeqScanOperator,
                    TableName = RecommendationEngineTestsConstants.TinyTable,
                    RelativeCost = RecommendationEngineTestsConstants.CheapRelativeCost,
                    PredicateColumns = { RecommendationEngineTestsConstants.XColumn }
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
            EstimatedTotalCost = RecommendationEngineTestsConstants.OrdersEstimatedTotalCost,
            EngineMissingIndexes =
            {
                new EngineMissingIndex
                {
                    Table = RecommendationEngineTestsConstants.OrdersTable,
                    ImpactPercent = RecommendationEngineTestsConstants.OrdersImpactPercent,
                    EqualityColumns = { RecommendationEngineTestsConstants.StatusColumn },
                    IncludeColumns = { RecommendationEngineTestsConstants.TotalColumn }
                }
            },
            Nodes =
            {
                new PlanNode
                {
                    Operator = RecommendationEngineTestsConstants.ClusteredIndexScanOperator,
                    TableName = RecommendationEngineTestsConstants.OrdersTable,
                    RelativeCost = RecommendationEngineTestsConstants.ClusteredIndexScanRelativeCost,
                    EstimatedRows = RecommendationEngineTestsConstants.OrdersEstimatedRows,
                    EstimatedRowsRead = RecommendationEngineTestsConstants.OrdersEstimatedRowsRead,
                    PredicateColumns = { RecommendationEngineTestsConstants.StatusColumn },
                    OutputColumns = { RecommendationEngineTestsConstants.TotalColumn, RecommendationEngineTestsConstants.CustomerIdColumn }
                }
            }
        };

        var recs = new RecommendationEngine().Analyze(plan);
        var rec = Assert.Single(recs);
        Assert.Equal(RecommendationEngineTestsConstants.OrdersTable, rec.Table);
        Assert.Equal(new[] { RecommendationEngineTestsConstants.StatusColumn }, rec.KeyColumns);
        // includes merged from both sources
        Assert.Contains(RecommendationEngineTestsConstants.TotalColumn, rec.IncludeColumns);
        Assert.Contains(RecommendationEngineTestsConstants.CustomerIdColumn, rec.IncludeColumns);
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
            Table = RecommendationEngineTestsConstants.OrdersTable,
            KeyColumns = new() { RecommendationEngineTestsConstants.StatusColumn, RecommendationEngineTestsConstants.CreatedAtColumn },
            IncludeColumns = { RecommendationEngineTestsConstants.TotalColumn }
        };
        var sql = rec.ToCreateStatement(PlanDialect.SqlServer);
        Assert.Contains(RecommendationEngineTestsConstants.CreateIndexStatement, sql);
        Assert.Contains(RecommendationEngineTestsConstants.IncludeStatement, sql);
    }
}
