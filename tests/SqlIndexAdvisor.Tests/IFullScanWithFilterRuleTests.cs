namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Interface for FullScanWithFilterRuleTests
/// </summary>
public interface IFullScanWithFilterRuleTests
{
    void Evaluate_SeqScanWithFilterPredicate_ReturnsRecommendation();
    void Evaluate_ClusteredIndexScanWithFilterPredicate_ReturnsRecommendation();
    void Evaluate_SeqScanWithoutPredicate_ReturnsNoRecommendation();
    void Evaluate_TableScanWithLowCost_ReturnsNoRecommendation();
    void Evaluate_IndexScanWithFilterPredicate_ReturnsRecommendation();
    void Evaluate_MultipleScans_ReturnsRecommendationForScanWithFilterOnly();
    void Evaluate_ScanWithHighCostAndSelectiveFilter_ReturnsHighConfidence();
    void Evaluate_ScanWithMediumCost_ReturnsMediumConfidence();
    void Evaluate_ScanWithLowCost_ReturnsLowConfidence();
    void Evaluate_ScanWithoutTableName_ReturnsNoRecommendation();
    void Name_ReturnsLowercaseRuleName();
}