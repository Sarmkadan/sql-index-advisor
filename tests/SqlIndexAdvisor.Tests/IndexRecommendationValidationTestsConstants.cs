namespace SqlIndexAdvisor.Tests;

internal static class IndexRecommendationValidationTestsConstants
{
    public const string DefaultTableName = "dbo.Users";
    public const string InvalidString = "   ";
    public const string ErrorTableMustBeNonEmpty = "Table property must be a non-empty string.";
    public const string ErrorKeyColumnsNotNull = "KeyColumns collection must not be null.";
    public const string ErrorKeyColumnsMustContainAtLeastOne = "KeyColumns collection must contain at least one column.";
    public const string ErrorKeyColumnsMustBeNonEmpty = "All KeyColumns must be non-empty strings.";
    public const string ErrorIncludeColumnsMustBeNonEmpty = "All IncludeColumns must be non-empty strings.";
    public const string ErrorEstimatedImpactPercentRange = "EstimatedImpactPercent must be between 0 and 100 inclusive.";
    public const string ErrorReasonsMustBeNonEmpty = "All Reasons must be non-empty strings.";
    public const string ErrorIndexRecommendationInvalid = "IndexRecommendation is invalid:";
    public const double MinEstimatedImpactPercent = 0.0;
    public const double MaxEstimatedImpactPercent = 100.0;
}
