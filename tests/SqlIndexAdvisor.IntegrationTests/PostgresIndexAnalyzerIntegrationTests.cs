using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.IntegrationTests;

/// <summary>
/// Integration tests for IndexAnalyzer with PostgreSQL execution plans.
/// </summary>
public class PostgresIndexAnalyzerIntegrationTests
{
    private readonly IPlanParser _parser = new PostgresJsonPlanParser();
    private readonly IRecommendationEngine _engine = new RecommendationEngine();

    /// <summary>
    /// Test that missing-index candidates are found for low-selectivity flag filters in PostgreSQL.
    /// </summary>
    [Fact]
    public void Analyze_PostgresSeqScanWithLowSelectivityFilter_ReturnsRecommendation()
    {
        // Arrange - PostgreSQL Seq Scan with WHERE is_active = true (low selectivity)
        var planContent = File.ReadAllText(
            "/home/personal/task-factory/workdir/sql-index-advisor/tests/SqlIndexAdvisor.IntegrationTests/Fixtures/PostgreSQL/01_key_lookup_with_filter.json");
        var plan = _parser.Parse(planContent).Single();

        // Act
        var recommendations = _engine.Analyze(plan);

        // Assert
        Assert.NotEmpty(recommendations);
        var rec = Assert.Single(recommendations);
        Assert.Equal("orders", rec.Table);
        // The parser extracts column names with quotes from the JSON
        Assert.Contains("\"status\"", rec.KeyColumns);
        Assert.Contains("\"created_at\"", rec.KeyColumns);
        Assert.Contains("\"customer_id\"", rec.KeyColumns);
        Assert.True(rec.EstimatedImpactPercent > 0);
        Assert.NotEmpty(rec.Reasons);
    }
}