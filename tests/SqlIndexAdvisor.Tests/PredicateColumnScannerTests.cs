using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the <see cref="PredicateColumnScanner"/> class.
/// </summary>
public class PredicateColumnScannerTests
{
    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method picks up equality columns.
    /// </summary>
    [Fact]
    public void PicksUpEqualityColumns()
    {
        var cols = PredicateColumnScanner.Scan("((country = 'PL'::text) AND (is_active = true))").ToList();
        Assert.Contains("country", cols);
        Assert.Contains("is_active", cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method strips alias prefix.
    /// </summary>
    [Fact]
    public void StripsAliasPrefix()
    {
        var cols = PredicateColumnScanner.Scan("(u.status = 'open')").ToList();
        Assert.Equal(new[] { "status" }, cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method ignores boolean keywords.
    /// </summary>
    [Fact]
    public void IgnoresBooleanKeywords()
    {
        var cols = PredicateColumnScanner.Scan("(a = 1 AND b > 2)").ToList();
        Assert.DoesNotContain("AND", cols);
        Assert.Equal(new[] { "a", "b" }, cols);
    }

    /// <summary>
    /// Verifies that the <see cref="PredicateColumnScanner.Scan(string)"/> method handles range and in operators.
    /// </summary>
    [Fact]
    public void HandlesRangeAndInOperators()
    {
        var cols = PredicateColumnScanner.Scan("(price BETWEEN 10 AND 20 AND category IN ('x','y'))").ToList();
        Assert.Contains("price", cols);
        Assert.Contains("category", cols);
    }
}
