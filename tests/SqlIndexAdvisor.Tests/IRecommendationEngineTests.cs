namespace SqlIndexAdvisor.Tests;

public interface IRecommendationEngineTests
{
    void SeqScanWithFilterProducesRecommendation();
    void CheapScanIsIgnored();
    void EngineHintAndScanOnSameKeysAreMerged();
    void CreateStatementIncludesKeysAndIncludes();
}
