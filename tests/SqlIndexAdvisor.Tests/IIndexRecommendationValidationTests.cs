namespace SqlIndexAdvisor.Tests;

public interface IIndexRecommendationValidationTests
{
    void Validate_WithValidRecommendation_ReturnsEmptyList();
    void Validate_WithNullTable_ReturnsError();
    void Validate_WithEmptyTable_ReturnsError();
    void Validate_WithNullKeyColumns_ReturnsError();
    void Validate_WithEmptyKeyColumns_ReturnsError();
    void Validate_WithWhitespaceKeyColumns_ReturnsError();
    void Validate_WithNullIncludeColumns_DoesNotAddError();
    void Validate_WithWhitespaceIncludeColumns_ReturnsError();
    void Validate_WithInvalidEstimatedImpactPercent_ReturnsError();
    void Validate_WithMaxEstimatedImpactPercent_ReturnsNoError();
    void Validate_WithNullReasons_DoesNotAddError();
    void Validate_WithWhitespaceReasons_ReturnsError();
    void Validate_WithMultipleProblems_ReturnsAllErrors();
    void IsValid_WithValidRecommendation_ReturnsTrue();
    void IsValid_WithInvalidRecommendation_ReturnsFalse();
    void IsValid_WithNullRecommendation_ThrowsArgumentNullException();
    void EnsureValid_WithValidRecommendation_DoesNotThrow();
    void EnsureValid_WithInvalidRecommendation_ThrowsArgumentException();
    void EnsureValid_WithNullRecommendation_ThrowsArgumentNullException();
    void Validate_WithNullRecommendation_ThrowsArgumentNullException();
}
