namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Holds constant values used by <see cref="ImplicitConversionRule"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class ImplicitConversionRuleConstants
{
    public const string RuleName = "implicit-conversion";

    // SQL Server showplan function that marks an implicit type conversion in a predicate.
    public const string ConvertImplicitFunction = "CONVERT_IMPLICIT";

    // Postgres cast operator; a cast on a column can block index usage when the types differ.
    public const string PostgresCastOperator = "::";
}
