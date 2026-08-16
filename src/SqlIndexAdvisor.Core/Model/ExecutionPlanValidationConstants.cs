namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Holds constant values used for validation messages and numeric limits
/// in <see cref="ExecutionPlanValidation"/>.
/// </summary>
internal static class ExecutionPlanValidationConstants
{
    // Validation message format strings
    public const string InvalidDialectMessage = "Invalid Dialect value: {0}. Expected SqlServer or Postgres.";
    public const string StatementTextCannotBeNull = "StatementText cannot be null.";
    public const string EstimatedTotalCostNaN = "EstimatedTotalCost cannot be NaN.";
    public const string EstimatedTotalCostInfinite = "EstimatedTotalCost cannot be infinite.";
    public const string EstimatedTotalCostNegative = "EstimatedTotalCost cannot be negative.";
    public const string NodesCollectionCannotBeNull = "Nodes collection cannot be null.";
    public const string NodeCannotBeNull = "Nodes[{0}] cannot be null.";
    public const string NodeOperatorCannotBeNullOrEmpty = "Nodes[{0}].Operator cannot be null or empty.";
    public const string NodeEstimatedRowsNegative = "Nodes[{0}].EstimatedRows cannot be negative. Actual: {1}";
    public const string NodeEstimatedRowsNaN = "Nodes[{0}].EstimatedRows cannot be NaN.";
    public const string NodeEstimatedRowsInfinite = "Nodes[{0}].EstimatedRows cannot be infinite.";
    public const string NodeEstimatedRowsReadNegative = "Nodes[{0}].EstimatedRowsRead cannot be negative. Actual: {1}";
    public const string NodeEstimatedRowsReadNaN = "Nodes[{0}].EstimatedRowsRead cannot be NaN.";
    public const string NodeEstimatedRowsReadInfinite = "Nodes[{0}].EstimatedRowsRead cannot be infinite.";
    public const string NodeRelativeCostOutOfRange = "Nodes[{0}].RelativeCost must be between 0 and 1. Actual: {1}";
    public const string NodeRelativeCostNaN = "Nodes[{0}].RelativeCost cannot be NaN.";
    public const string NodeRelativeCostInfinite = "Nodes[{0}].RelativeCost cannot be infinite.";
    public const string EngineMissingIndexesCollectionCannotBeNull = "EngineMissingIndexes collection cannot be null.";
    public const string EngineMissingIndexCannotBeNull = "EngineMissingIndexes[{0}] cannot be null.";
    public const string EngineMissingIndexTableCannotBeNullOrEmpty = "EngineMissingIndexes[{0}].Table cannot be null or empty.";
    public const string EngineMissingIndexImpactPercentOutOfRange = "EngineMissingIndexes[{0}].ImpactPercent must be between 0 and 100. Actual: {1}";
    public const string EngineMissingIndexImpactPercentNaN = "EngineMissingIndexes[{0}].ImpactPercent cannot be NaN.";
    public const string EngineMissingIndexImpactPercentInfinite = "EngineMissingIndexes[{0}].ImpactPercent cannot be infinite.";
    public const string CollectionCannotBeNull = "{0} cannot be null.";
    public const string CollectionItemCannotBeNullOrEmpty = "{0}[{1}] cannot be null or empty.";

    // Numeric limits
    public const double RelativeCostMin = 0.0;
    public const double RelativeCostMax = 1.0;
    public const double ImpactPercentMin = 0.0;
    public const double ImpactPercentMax = 100.0;
}
