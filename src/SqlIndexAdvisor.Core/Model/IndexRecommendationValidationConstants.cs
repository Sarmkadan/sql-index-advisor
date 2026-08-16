namespace SqlIndexAdvisor.Core.Model;

internal static class IndexRecommendationValidationConstants
{
    public const string TablePropertyMustBeNonEmptyString = "Table property must be a non-empty string.";
    public const string KeyColumnsCollectionMustNotBeNull = "KeyColumns collection must not be null.";
    public const string KeyColumnsCollectionMustContainAtLeastOneColumn = "KeyColumns collection must contain at least one column.";
    public const string AllKeyColumnsMustBeNonEmptyStrings = "All KeyColumns must be non-empty strings.";
    public const string AllIncludeColumnsMustBeNonEmptyStrings = "All IncludeColumns must be non-empty strings.";
    public const string EstimatedImpactPercentMustBeBetween0And100Inclusive = "EstimatedImpactPercent must be between 0 and 100 inclusive.";
    public const string AllReasonsMustBeNonEmptyStrings = "All Reasons must be non-empty strings.";
    public const int MinEstimatedImpactPercent = 0;
    public const int MaxEstimatedImpactPercent = 100;
}
