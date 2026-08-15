namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Interface for the <see cref="PredicateColumnScannerTests"/> class.
/// </summary>
public interface IPredicateColumnScannerTests
{
    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method picks up equality columns.
    /// </summary>
    void PicksUpEqualityColumns();

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method strips alias prefix.
    /// </summary>
    void StripsAliasPrefix();

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method ignores boolean keywords.
    /// </summary>
    void IgnoresBooleanKeywords();

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method handles range and in operators.
    /// </summary>
    void HandlesRangeAndInOperators();
}
