using System.Text;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Model;

public enum Confidence
{
    Low,
    Medium,
    High
}

/// <summary>
/// Distinguishes between recommendations that suggest creating an index versus those that
/// recommend fixing the query or schema (e.g., implicit conversions, parameter sniffing).
/// </summary>
public enum RecommendationKind
{
    /// <summary>Create a new index to improve query performance.</summary>
    CreateIndex,

    /// <summary>Fix the query or schema (e.g., implicit conversion, parameter type mismatch).</summary>
    SchemaFix
}

/// <summary>
/// A single suggested index plus the reasoning that produced it. The estimated
/// impact is deliberately "rough" - it is a heuristic score, not a promise.
/// </summary>
public sealed class IndexRecommendation : IIndexRecommendation
{
    public required string Table { get; init; }
    public required List<string> KeyColumns { get; init; }
    public List<string> IncludeColumns { get; init; } = new();

    /// <summary>0..100. Rough share of statement cost we expect this to remove.</summary>
    public double EstimatedImpactPercent { get; init; }

    /// <summary>Fraction of the whole statement cost attributed to the source plan node (0..1).</summary>
    public double SourceNodeCost { get; init; }

    public Confidence Confidence { get; init; }

    /// <summary>Name of the rule that produced this recommendation. Stamped by the engine.</summary>
    public string? Rule { get; set; }

    /// <summary>Which rule(s) fired to produce this recommendation.</summary>
    public List<string> Reasons { get; init; } = new();

    ///
    /// The kind of recommendation this is. Determines whether it suggests creating an index
    /// or fixing a query/schema issue.
    /// </summary>
    public RecommendationKind Kind { get; init; } = RecommendationKind.CreateIndex;

    /// <summary>
    /// The name of an existing index that should be widened with INCLUDE columns,
    /// or null if a new index should be created.
    /// </summary>
    public string? ExistingIndexName { get; init; }

    public string SuggestedName(string? existingIndexName = null)
    {
        // If a specific existing index name is provided as parameter, use it
        if (existingIndexName != null)
        {
            return existingIndexName;
        }

        // Otherwise, use the stored ExistingIndexName if available
        if (ExistingIndexName != null)
        {
            return ExistingIndexName;
        }

        // Fall back to generating a new index name
        var cols = string.Join("_", KeyColumns.Select(Sanitize));
        var bare = Sanitize(Table.Split('.').Last());
        return $"IX_{bare}_{cols}";
    }

    /// <summary>
    /// Generates a dialect-specific CREATE INDEX statement from this recommendation.
    /// </summary>
    /// <param name="dialect">The target SQL dialect (SQL Server or Postgres).</param>
    /// <returns>A ready-to-run CREATE INDEX statement string with proper dialect-specific syntax and options.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported dialect is provided.</exception>
    public string ToCreateStatement(PlanDialect dialect)
    {
        return DdlRenderer.RenderCreateIndex(this, dialect);
    }

    private static string Sanitize(string raw) =>
        new string(raw.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
}
