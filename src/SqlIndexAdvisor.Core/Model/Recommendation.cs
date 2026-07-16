using System.Text;

namespace SqlIndexAdvisor.Core.Model;

public enum Confidence
{
    Low,
    Medium,
    High
}

/// <summary>
/// A single suggested index plus the reasoning that produced it. The estimated
/// impact is deliberately "rough" - it is a heuristic score, not a promise.
/// </summary>
public sealed class IndexRecommendation
{
    public required string Table { get; init; }
    public required List<string> KeyColumns { get; init; }
    public List<string> IncludeColumns { get; init; } = new();

    /// <summary>0..100. Rough share of statement cost we expect this to remove.</summary>
    public double EstimatedImpactPercent { get; init; }

    public Confidence Confidence { get; init; }

    /// <summary>Which rule(s) fired to produce this recommendation.</summary>
    public List<string> Reasons { get; init; } = new();

    public string SuggestedName()
    {
        var cols = string.Join("_", KeyColumns.Select(Sanitize));
        var bare = Sanitize(Table.Split('.').Last());
        return $"IX_{bare}_{cols}";
    }

    public string ToCreateStatement(PlanDialect dialect)
    {
        var sb = new StringBuilder();
        sb.Append("CREATE INDEX ").Append(SuggestedName());
        sb.Append(" ON ").Append(Table);
        sb.Append(" (").Append(string.Join(", ", KeyColumns)).Append(')');

        if (IncludeColumns.Count > 0)
        {
            // Same syntax in both dialects: Postgres spells covering columns
            // as INCLUDE too (11+). The dialect parameter stays so a future
            // dialect with different syntax has somewhere to branch.
            _ = dialect;
            sb.Append(" INCLUDE (").Append(string.Join(", ", IncludeColumns)).Append(')');
        }

        sb.Append(';');
        return sb.ToString();
    }

    private static string Sanitize(string raw) =>
        new string(raw.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
}
