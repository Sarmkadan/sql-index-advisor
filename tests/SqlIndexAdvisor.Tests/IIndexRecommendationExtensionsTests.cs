using System;

namespace SqlIndexAdvisor.Tests
{
    public interface IIndexRecommendationExtensionsTests
    {
        void ContainsColumn_WithExistingKeyColumn_ReturnsTrue();
        void ContainsColumn_WithExistingIncludeColumn_ReturnsTrue();
        void ContainsColumn_WithNonExistingColumn_ReturnsFalse();
        void ContainsColumn_WithCaseInsensitiveMatch_ReturnsTrue();
        void ContainsColumn_WithEmptyKeyColumns_ReturnsFalse();
        void GetTotalColumnCount_WithBothKeyAndIncludeColumns_ReturnsCorrectCount();
        void GetTotalColumnCount_WithOnlyKeyColumns_ReturnsKeyCount();
        void GetTotalColumnCount_WithOnlyIncludeColumns_ReturnsIncludeCount();
        void GetTotalColumnCount_WithEmptyCollections_ReturnsZero();
        void ToDisplayString_WithValidRecommendation_ReturnsFormattedString();
        void ToDisplayString_WithOnlyKeyColumns_ReturnsCorrectFormat();
        void ToDisplayString_WithOnlyIncludeColumns_ReturnsCorrectFormat();
        void ToSummaryString_WithValidRecommendation_ReturnsConciseString();
        void ToSummaryString_WithOnlyKeyColumns_ReturnsConciseFormat();
        void ToSummaryString_WithOnlyIncludeColumns_ReturnsConciseFormat();
        void ToSummaryString_WithSingleColumn_ReturnsCorrectFormat();
        void ContainsColumn_WithNullRecommendation_ThrowsArgumentNullException();
        void ContainsColumn_WithNullColumnName_ThrowsArgumentNullException();
        void ContainsColumn_WithEmptyColumnName_ThrowsArgumentException();
        void ContainsColumn_WithWhitespaceColumnName_DoesNotThrow();
    }
}
