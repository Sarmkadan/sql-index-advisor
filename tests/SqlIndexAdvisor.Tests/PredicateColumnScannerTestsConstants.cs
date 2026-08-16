namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Holds constant values used in <see cref="PredicateColumnScannerTests"/> to avoid magic strings.
/// </summary>
internal static class PredicateColumnScannerTestsConstants
{
    // SQL query strings
    public const string EqualityColumnsSql = "((country = 'PL'::text) AND (is_active = true))";
    public const string AliasPrefixSql = "(u.status = 'open')";
    public const string BooleanKeywordsSql = "(a = 1 AND b > 2)";
    public const string RangeAndInSql = "(price BETWEEN 10 AND 20 AND category IN ('x','y'))";

    // Expected column names
    public const string EqualityColumnCountry = "country";
    public const string EqualityColumnIsActive = "is_active";

    public const string AliasPrefixColumn = "status";

    public const string BooleanKeywordAnd = "AND";
    public const string BooleanColumnA = "a";
    public const string BooleanColumnB = "b";

    public const string RangeColumnPrice = "price";
    public const string RangeColumnCategory = "category";
}
