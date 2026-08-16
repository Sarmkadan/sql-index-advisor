using System;

namespace SqlIndexAdvisor.Tests
{
    public interface IExecutionPlanValidationTests
    {
        void Validate_HappyPath_ForEachMajorPublicMethod_ReturnsNoProblems();
        void Validate_NullInput_ThrowsArgumentNullException();
        void IsValid_HappyPath_ForEachMajorPublicMethod_ReturnsTrue();
        void IsValid_NullInput_ThrowsArgumentNullException();
        void EnsureValid_HappyPath_ForEachMajorPublicMethod_DoesNotThrow();
        void EnsureValid_NullInput_ThrowsArgumentNullException();
        void EnsureValid_InvalidPlan_ThrowsArgumentException();
        void Validate_InvalidDialect_ReturnsProblem();
        void Validate_NaNEstimatedTotalCost_ReturnsProblem();
        void Validate_InfiniteEstimatedTotalCost_ReturnsProblem();
        void Validate_NullNodesCollection_ReturnsProblem();
        void Validate_NullNodeInCollection_ReturnsProblem();
        void Validate_EmptyOperator_ReturnsProblem();
        void Validate_NegativeEstimatedRows_ReturnsProblem();
        void Validate_NaNEstimatedRows_ReturnsProblem();
        void Validate_OutOfRangeRelativeCost_ReturnsProblem();
        void Validate_NullPredicateColumnsCollection_ReturnsProblem();
        void Validate_EmptyPredicateColumns_ReturnsNoProblem();
        void Validate_NullEngineMissingIndexesCollection_ReturnsProblem();
        void Validate_NullMissingIndex_ReturnsProblem();
    }
}
