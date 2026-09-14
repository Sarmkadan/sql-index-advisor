using System.Text;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Represents the confidence level of a recommendation.
/// </summary>
public enum Confidence
{
    /// <summary>Low confidence.</summary>
    Low,
    /// <summary>Medium confidence.</summary>
    Medium,
    /// <summary>High confidence.</summary>
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
    /// <summary>
    /// The name of the table on which the index should be created.
    /// </summary>
    public required string Table { get; init; }

    /// <summary>
    /// The list of column names that form the key of the index.
    /// </summary>
    public required List<string> KeyColumns { get; init; }

    /// <summary>
    /// The list of column names to include in the index as non-key (included) columns.
    /// </summary>
    public List<string> IncludeColumns { get; init; } = new();

    /// <summary>
    /// Estimated percentage of statement cost that this index is expected to remove (0-100).
    /// </summary>
    public double EstimatedImpactPercent { get; init; }

    /// <summary>
    /// Fraction of the whole statement cost attributed to the source plan node (0-1).
    /// </summary>
    public double SourceNodeCost { get; init; }

    /// <summary>
    /// The confidence level in this recommendation.
    /// </summary>
    public Confidence Confidence { get; init; }

    /// <summary>
    /// The name of the rule that generated this recommendation. Set by the engine.
    /// </summary>
    public string? Rule { get; set; }

    /// <summary>
    /// A list of rule names that contributed to producing this recommendation.
    /// </summary>
    public List<string> Reasons { get; init; } = new();

    /// <summary>
    /// The type of recommendation: whether to create an index or fix the query/schema.
    /// </summary>
    public RecommendationKind Kind { get; init; } = RecommendationKind.CreateIndex;

    /// <summary>
    /// The name of an existing index that should be widened with INCLUDE columns,
    /// or null if a new index should be created.
    /// </summary>
    public string? ExistingIndexName { get; init; }

    /// <summary>
    /// Generates a suggested index name based on the table and key columns, or uses an existing index name if provided.
    /// </summary>
    /// <param name="existingIndexName">Optional existing index name to use instead of generating a new one.</param>
    /// <returns>A string suitable for use as an index name.</returns>
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

    /// <summary>
    /// Returns a concise human-readable summary of the recommendation's key fields.
    /// </summary>
    /// <returns>A string summarizing the recommendation.</returns>
    public override string ToString()
    {
        var keyColumnsPart = KeyColumns.Any() ? $"({string.Join(", ", KeyColumns)})" : "";
        var includeColumnsPart = IncludeColumns.Any() ? $" INCLUDE ({string.Join(", ", IncludeColumns)})" : "";

        string recommendationPart;
        if (Kind == RecommendationKind.CreateIndex)
        {
            recommendationPart = $"INDEX ON {Table}{keyColumnsPart}{includeColumnsPart}";
        }
        else
        {
            recommendationPart = $"SCHEMA FIX for {Table}";
        }

        return $"{recommendationPart} - Impact: {EstimatedImpactPercent}%, Confidence: {Confidence}";
    }

    private static string Sanitize(string raw) =>
        new string(raw.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
}