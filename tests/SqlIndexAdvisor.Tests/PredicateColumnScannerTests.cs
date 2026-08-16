using SqlIndexAdvisor.Core.Parsing;
using Xunit;
using static SqlIndexAdvisor.Tests.PredicateColumnScannerTestsConstants;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the <see cref="PredicateColumnScanner"/> class.
/// </summary>
public class PredicateColumnScannerTests : IPredicateColumnScannerTests
{
    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method picks up equality columns.
    /// </summary>
    [Fact]
    public void PicksUpEqualityColumns()
    {
        var cols = PredicateColumnScanner.Scan(EqualityColumnsSql).ToList();
        Assert.Contains(EqualityColumnCountry, cols);
        Assert.Contains(EqualityColumnIsActive, cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method strips alias prefix.
    /// </summary>
    [Fact]
    public void StripsAliasPrefix()
    {
        var cols = PredicateColumnScanner.Scan(AliasPrefixSql).ToList();
        Assert.Equal(new[] { AliasPrefixColumn }, cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method ignores boolean keywords.
    /// </summary>
    [Fact]
    public void IgnoresBooleanKeywords()
    {
        var cols = PredicateColumnScanner.Scan(BooleanKeywordsSql).ToList();
        Assert.DoesNotContain(BooleanKeywordAnd, cols);
        Assert.Equal(new[] { BooleanColumnA, BooleanColumnB }, cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method handles range and in operators.
    /// </summary>
    [Fact]
    public void HandlesRangeAndInOperators()
    {
        var cols = PredicateColumnScanner.Scan(RangeAndInSql).ToList();
        Assert.Contains(RangeColumnPrice, cols);
        Assert.Contains(RangeColumnCategory, cols);
    }
}
