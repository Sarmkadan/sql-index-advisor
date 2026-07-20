using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Runs every rule against a plan, then de-duplicates. Two recommendations are
/// considered the same index if they target the same table with the same key
/// columns in the same order (a prefix match is treated as a dup - the wider one
/// wins and absorbs the other's includes/reasons).
/// </summary>
public sealed class RecommendationEngine
{
    private readonly IReadOnlyList<IIndexRule> _rules;

    public RecommendationEngine()
        : this(new IIndexRule[] { new EngineHintRule(), new FullScanWithFilterRule(), new ExpensiveSortRule() })
    {
    }

    public RecommendationEngine(IEnumerable<IIndexRule> rules) => _rules = rules.ToList();

    public IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan)
    {
        var raw = _rules.SelectMany(r => r.Evaluate(plan)).ToList();
        var merged = Merge(raw);
        return merged
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.EstimatedImpactPercent)
            .ToList();
    }

    private static List<IndexRecommendation> Merge(List<IndexRecommendation> raw)
    {
        var kept = new List<IndexRecommendation>();

        foreach (var candidate in raw)
        {
            var dupIndex = kept.FindIndex(k => IsSameOrPrefix(k, candidate));
            if (dupIndex < 0)
            {
                kept.Add(candidate);
                continue;
            }

            var existing = kept[dupIndex];
            // Keep the one with more key columns (wider covers the narrower).
            var winner = candidate.KeyColumns.Count >= existing.KeyColumns.Count ? candidate : existing;
            var loser = ReferenceEquals(winner, candidate) ? existing : candidate;

            var includes = winner.IncludeColumns
                .Concat(loser.IncludeColumns)
                .Where(c => !winner.KeyColumns.Contains(c))
                .Distinct()
                .ToList();

            kept[dupIndex] = new IndexRecommendation
            {
                Table = winner.Table,
                KeyColumns = winner.KeyColumns,
                IncludeColumns = includes,
                EstimatedImpactPercent = Math.Max(winner.EstimatedImpactPercent, loser.EstimatedImpactPercent),
                Confidence = (Confidence)Math.Max((int)winner.Confidence, (int)loser.Confidence),
                Reasons = winner.Reasons.Concat(loser.Reasons).Distinct().ToList()
            };
        }

        return kept;
    }

    private static bool IsSameOrPrefix(IndexRecommendation a, IndexRecommendation b)
    {
        if (!string.Equals(a.Table, b.Table, StringComparison.OrdinalIgnoreCase)) return false;
        var min = Math.Min(a.KeyColumns.Count, b.KeyColumns.Count);
        if (min == 0) return false;
        for (var i = 0; i < min; i++)
            if (!string.Equals(a.KeyColumns[i], b.KeyColumns[i], StringComparison.OrdinalIgnoreCase))
                return false;
        return true;
    }
}
