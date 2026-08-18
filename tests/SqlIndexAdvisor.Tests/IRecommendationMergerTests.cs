namespace SqlIndexAdvisor.Tests;

public interface IRecommendationMergerTests
{
    void Merge_WithPrefixColumns_MergesCorrectly();
    void Merge_WithSameColumns_MergesCorrectly();
    void Merge_WithDifferentTables_DoesNotMerge();
    void Merge_WithNonPrefixColumns_DoesNotMerge();
    void Merge_EmptyList_ReturnsEmptyList();
    void Merge_SingleRecommendation_ReturnsSame();
}
