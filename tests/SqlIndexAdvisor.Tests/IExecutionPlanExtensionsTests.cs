namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Contract for the ExecutionPlanExtensions test suite.
/// </summary>
public interface IExecutionPlanExtensionsTests
{
    void GetScanCandidates_WithValidPlan_ReturnsScanNodes();

    void GetScanCandidates_WithIndexOnlyScans_ReturnsEmpty();

    void GetScanCandidates_WithEmptyPlan_ReturnsEmpty();

    void GetScanCandidates_WithNullPlan_ThrowsArgumentNullException();

    void GetTotalScanCost_WithValidPlan_ReturnsSumOfScanCosts();

    void GetTotalScanCost_WithEmptyPlan_ReturnsZero();

    void GetTotalScanCost_WithNullPlan_ThrowsArgumentNullException();

    void GetScannedTables_WithValidPlan_ReturnsDistinctTableNames();

    void GetScannedTables_WithEmptyPlan_ReturnsEmpty();

    void GetScannedTables_WithNullPlan_ThrowsArgumentNullException();

    void GetPredicateColumns_WithValidPlan_ReturnsDistinctPredicateColumns();

    void GetPredicateColumns_WithEmptyPlan_ReturnsEmpty();

    void GetPredicateColumns_WithNullPlan_ThrowsArgumentNullException();

    void GetIncludeCandidateColumns_WithValidPlan_ReturnsNonPredicateOutputColumns();

    void GetIncludeCandidateColumns_WithEmptyPlan_ReturnsEmpty();

    void GetIncludeCandidateColumns_WithNullPlan_ThrowsArgumentNullException();

    void GetMissingIndexEqualityColumns_WithValidPlan_ReturnsEqualityColumns();

    void GetMissingIndexEqualityColumns_WithEmptyPlan_ReturnsEmpty();

    void GetMissingIndexEqualityColumns_WithNullPlan_ThrowsArgumentNullException();

    void GetMissingIndexInequalityColumns_WithValidPlan_ReturnsInequalityColumns();
}
