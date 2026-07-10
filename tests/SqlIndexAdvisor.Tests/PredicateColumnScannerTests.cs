using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

public class PredicateColumnScannerTests
{
    [Fact]
    public void PicksUpEqualityColumns()
    {
        var cols = PredicateColumnScanner.Scan("((country = 'PL'::text) AND (is_active = true))").ToList();
        Assert.Contains("country", cols);
        Assert.Contains("is_active", cols);
    }

    [Fact]
    public void StripsAliasPrefix()
    {
        var cols = PredicateColumnScanner.Scan("(u.status = 'open')").ToList();
        Assert.Equal(new[] { "status" }, cols);
    }

    [Fact]
    public void IgnoresBooleanKeywords()
    {
        var cols = PredicateColumnScanner.Scan("(a = 1 AND b > 2)").ToList();
        Assert.DoesNotContain("AND", cols);
        Assert.Equal(new[] { "a", "b" }, cols);
    }

    [Fact]
    public void HandlesRangeAndInOperators()
    {
        var cols = PredicateColumnScanner.Scan("(price BETWEEN 10 AND 20 AND category IN ('x','y'))").ToList();
        Assert.Contains("price", cols);
        Assert.Contains("category", cols);
    }
}
