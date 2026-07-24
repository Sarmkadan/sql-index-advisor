using System.Globalization;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Reporting;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="ReportRendererTests"/> to provide reusable test utilities
/// for ReportRenderer functionality validation.
/// </summary>
public static class ReportRendererTestsExtensions
{
    /// <summary>
    /// Creates a standard execution plan for testing purposes.
    /// </summary>
    /// <param name="dialect">The SQL dialect to use.</param>
    /// <param name="estimatedCost">The estimated total cost of the plan.</param>
    /// <param name="tableNames">Optional table names to include in plan nodes.</param>
    /// <returns>A configured <see cref="ExecutionPlan"/> instance.</returns>
    public static ExecutionPlan CreateTestPlan(this ReportRendererTests _, PlanDialect dialect, double estimatedCost, params ReadOnlySpan<string> tableNames)
    {
        var plan = new ExecutionPlan
        {
            Dialect = dialect,
            EstimatedTotalCost = estimatedCost,
            Nodes = new()
        };

        foreach (var tableName in tableNames)
        {
            plan.Nodes.Add(new PlanNode { Operator = "Seq Scan", TableName = tableName });
        }

        return plan;
    }

    /// <summary>
    /// Creates a standard index recommendation for testing purposes.
    /// </summary>
    /// <param name="table">The table name.</param>
    /// <param name="keyColumns">The key columns for the index.</param>
    /// <param name="includeColumns">Optional include columns for the index.</param>
    /// <param name="estimatedImpactPercent">The estimated impact percentage.</param>
    /// <param name="confidence">The confidence level.</param>
    /// <param name="reasons">Optional reasons for the recommendation.</param>
    /// <returns>A configured <see cref="IndexRecommendation"/> instance.</returns>
    public static IndexRecommendation CreateTestRecommendation(
        this ReportRendererTests _,
        string table,
        IReadOnlyList<string> keyColumns,
        IReadOnlyList<string>? includeColumns = null,
        double estimatedImpactPercent = 50.0,
        Confidence confidence = Confidence.Medium,
        params string[] reasons)
    {
        ArgumentException.ThrowIfNullOrEmpty(table);
        ArgumentNullException.ThrowIfNull(keyColumns);

        var recommendation = new IndexRecommendation
        {
            Table = table,
            KeyColumns = new(keyColumns),
            EstimatedImpactPercent = estimatedImpactPercent,
            Confidence = confidence
        };

        if (reasons is not null)
        {
            recommendation.Reasons.AddRange(reasons);
        }

        if (includeColumns is not null)
        {
            recommendation.IncludeColumns.AddRange(includeColumns);
        }

        return recommendation;
    }

    /// <summary>
    /// Creates a list of recommendations from the provided parameters.
    /// </summary>
    /// <param name="recommendations">The recommendations to include in the list.</param>
    /// <returns>A new <see cref="List{T}"/> containing the recommendations.</returns>
    public static List<IndexRecommendation> CreateRecommendationList(this ReportRendererTests _, params IndexRecommendation[] recommendations)
    {
        ArgumentNullException.ThrowIfNull(recommendations);
        return new List<IndexRecommendation>(recommendations);
    }

    /// <summary>
    /// Extracts the CREATE INDEX statement from text output.
    /// </summary>
    /// <param name="output">The text output from ReportRenderer.RenderText.</param>
    /// <returns>The CREATE INDEX statement if found, otherwise null.</returns>
    public static string? ExtractCreateStatement(this ReportRendererTests _, string output)
    {
        ArgumentException.ThrowIfNullOrEmpty(output);

        var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("CREATE INDEX", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the INCLUDE clause from text output.
    /// </summary>
    /// <param name="output">The text output from ReportRenderer.RenderText.</param>
    /// <returns>The INCLUDE clause if found, otherwise null.</returns>
    public static string? ExtractIncludeClause(this ReportRendererTests _, string output)
    {
        ArgumentException.ThrowIfNullOrEmpty(output);

        var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("INCLUDE", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }

        return null;
    }
}