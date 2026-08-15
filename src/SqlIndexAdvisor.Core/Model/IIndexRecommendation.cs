using System.Collections.Generic;

namespace SqlIndexAdvisor.Core.Model;

public interface IIndexRecommendation
{
    string Table { get; init; }
    List<string> KeyColumns { get; init; }
    List<string> IncludeColumns { get; init; }
    double EstimatedImpactPercent { get; init; }
    double SourceNodeCost { get; init; }
    Confidence Confidence { get; init; }
    List<string> Reasons { get; init; }

    string SuggestedName(string? existingIndexName = null);
    string ToCreateStatement(PlanDialect dialect);
}
