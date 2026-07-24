using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Comprehensive edge case tests for the <see cref="PredicateColumnScanner"/> class.
/// Tests cover multi-column predicates, OR predicates, function-wrapped columns,
/// qualified column references, computed expressions, and various literal/parameter scenarios.
/// </summary>
public class PredicateColumnScannerEdgeCasesTests
{
    #region Multi-column AND predicates

    /// <summary>
    /// Verifies that the scanner correctly extracts multiple columns from AND predicates.
    /// </summary>
    [Fact]
    public void ExtractsMultipleColumnsFromAndPredicates()
    {
        var cols = PredicateColumnScanner.Scan("(country = 'PL' AND is_active = true)").ToList();
        Assert.Equal(new[] { "country", "is_active" }, cols);
    }

    /// <summary>
    /// Verifies that the scanner handles complex AND predicates with multiple operators.
    /// </summary>
    [Fact]
    public void HandlesComplexAndPredicatesWithMultipleOperators()
    {
        var cols = PredicateColumnScanner.Scan("(price > 10 AND quantity < 100 AND status = 'active')").ToList();
        Assert.Equal(new[] { "price", "quantity", "status" }, cols);
    }

    /// <summary>
    /// Verifies that the scanner handles nested AND predicates.
    /// </summary>
    [Fact]
    public void HandlesNestedAndPredicates()
    {
        var cols = PredicateColumnScanner.Scan("((a = 1 AND b = 2) AND (c = 3 AND d = 4))").ToList();
        Assert.Equal(new[] { "a", "b", "c", "d" }, cols);
    }

    #endregion

    #region OR predicates

    /// <summary>
    /// Verifies that the scanner extracts columns from OR predicates.
    /// Note: OR predicates are treated the same as AND for index recommendation purposes.
    /// </summary>
    [Fact]
    public void ExtractsColumnsFromOrPredicates()
    {
        var cols = PredicateColumnScanner.Scan("(status = 'open' OR status = 'pending')").ToList();
        Assert.Equal(new[] { "status", "status" }, cols);
    }

    /// <summary>
    /// Verifies that the scanner handles mixed AND/OR predicates.
    /// </summary>
    [Fact]
    public void HandlesMixedAndOrPredicates()
    {
        var cols = PredicateColumnScanner.Scan("(a = 1 AND (b = 2 OR c = 3))").ToList();
        Assert.Equal(new[] { "a", "b", "c" }, cols);
    }

    #endregion

    #region Function-wrapped columns

    /// <summary>
    /// Verifies that function-wrapped columns (non-sargable predicates) are NOT extracted.
    /// UPPER(col) = 'X' should not suggest an index since it can't be seekable.
    /// </summary>
    [Fact]
    public void DoesNotExtractFunctionWrappedColumns()
    {
        var cols = PredicateColumnScanner.Scan("(UPPER(name) = 'JOHN')").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that function-wrapped columns with different functions are not extracted.
    /// </summary>
    [Fact]
    public void DoesNotExtractFunctionWrappedColumnsWithVariousFunctions()
    {
        var cols = PredicateColumnScanner.Scan("(LOWER(email) = 'test@example.com')").ToList();
        Assert.Empty(cols);

        cols = PredicateColumnScanner.Scan("(TRIM(name) = 'John')").ToList();
        Assert.Empty(cols);

        cols = PredicateColumnScanner.Scan("(SUBSTRING(code, 1, 2) = 'AB')").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that nested function calls are not extracted.
    /// </summary>
    [Fact]
    public void DoesNotExtractNestedFunctionCalls()
    {
        var cols = PredicateColumnScanner.Scan("(CONCAT(UPPER(first_name), ' ', LOWER(last_name)) = 'JOHN DOE')").ToList();
        Assert.Empty(cols);
    }

    #endregion

    #region Qualified column references

    /// <summary>
    /// Verifies that qualified column references with table aliases are properly extracted.
    /// </summary>
    [Fact]
    public void ExtractsQualifiedColumnReferencesWithAliases()
    {
        var cols = PredicateColumnScanner.Scan("(u.status = 'open')").ToList();
        Assert.Equal(new[] { "status" }, cols);
    }

    /// <summary>
    /// Verifies that fully qualified column references with schema.table.column format are extracted.
    /// </summary>
    [Fact]
    public void ExtractsFullyQualifiedColumnReferences()
    {
        var cols = PredicateColumnScanner.Scan("(dbo.Users.status = 'active')").ToList();
        Assert.Equal(new[] { "status" }, cols);
    }

    /// <summary>
    /// Verifies that qualified references with different table aliases work correctly.
    /// </summary>
    [Fact]
    public void HandlesDifferentTableAliases()
    {
        var cols = PredicateColumnScanner.Scan("(t1.column1 = 1 AND t2.column2 = 2)").ToList();
        Assert.Equal(new[] { "column1", "column2" }, cols);
    }

    #endregion

    #region Computed/expression predicates

    /// <summary>
    /// Verifies that computed expressions without bare columns are not extracted.
    /// </summary>
    [Fact]
    public void DoesNotExtractComputedExpressionPredicates()
    {
        var cols = PredicateColumnScanner.Scan("(1 + 2 = 3)").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that complex arithmetic expressions are not extracted.
    /// </summary>
    [Fact]
    public void DoesNotExtractArithmeticExpressions()
    {
        var cols = PredicateColumnScanner.Scan("(price * quantity > 1000)").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that column references in complex expressions are still extracted when valid.
    /// </summary>
    [Fact]
    public void ExtractsColumnsFromValidExpressions()
    {
        var cols = PredicateColumnScanner.Scan("(price > 0 AND (price * quantity) > 100)").ToList();
        Assert.Equal(new[] { "price", "price" }, cols);
    }

    #endregion

    #region String literals and parameters

    /// <summary>
    /// Verifies that string literals are not mistaken for column names.
    /// </summary>
    [Fact]
    public void DoesNotExtractStringLiterals()
    {
        var cols = PredicateColumnScanner.Scan("(name = 'John')").ToList();
        Assert.Equal(new[] { "name" }, cols);
    }

    /// <summary>
    /// Verifies that numeric literals are not mistaken for column names.
    /// </summary>
    [Fact]
    public void DoesNotExtractNumericLiterals()
    {
        var cols = PredicateColumnScanner.Scan("(age = 25 AND price = 19.99)").ToList();
        Assert.Equal(new[] { "age", "price" }, cols);
    }

    /// <summary>
    /// Verifies that boolean literals are not mistaken for column names.
    /// </summary>
    [Fact]
    public void DoesNotExtractBooleanLiterals()
    {
        var cols = PredicateColumnScanner.Scan("(is_active = true AND is_deleted = false)").ToList();
        Assert.Equal(new[] { "is_active", "is_deleted" }, cols);
    }

    /// <summary>
    /// Verifies that NULL literals are not mistaken for column names.
    /// </summary>
    [Fact]
    public void DoesNotExtractNullLiterals()
    {
        var cols = PredicateColumnScanner.Scan("(middle_name IS NULL AND last_name IS NOT NULL)").ToList();
        Assert.Equal(new[] { "middle_name", "last_name" }, cols);
    }

    #endregion

    #region Various comparison operators

    /// <summary>
    /// Verifies that all comparison operators work correctly.
    /// </summary>
    [Fact]
    public void HandlesAllComparisonOperators()
    {
        var cols = PredicateColumnScanner.Scan("(col1 = 1 AND col2 <> 2 AND col3 != 3 AND col4 <= 4 AND col5 >= 5 AND col6 < 6 AND col7 > 7)").ToList();
        Assert.Equal(new[] { "col1", "col2", "col3", "col4", "col5", "col6", "col7" }, cols);
    }

    /// <summary>
    /// Verifies that LIKE operators work correctly.
    /// </summary>
    [Fact]
    public void HandlesLikeOperators()
    {
        var cols = PredicateColumnScanner.Scan("(name LIKE 'J%' AND email ILIKE '%@example.com')").ToList();
        Assert.Equal(new[] { "name", "email" }, cols);
    }

    /// <summary>
    /// Verifies that IN operators work correctly.
    /// </summary>
    [Fact]
    public void HandlesInOperators()
    {
        var cols = PredicateColumnScanner.Scan("(status IN ('active', 'pending') AND category IN (1, 2, 3))").ToList();
        Assert.Equal(new[] { "status", "category" }, cols);
    }

    /// <summary>
    /// Verifies that BETWEEN operators work correctly.
    /// </summary>
    [Fact]
    public void HandlesBetweenOperators()
    {
        var cols = PredicateColumnScanner.Scan("(age BETWEEN 18 AND 65 AND price BETWEEN 10.0 AND 100.0)").ToList();
        Assert.Equal(new[] { "age", "price" }, cols);
    }

    #endregion

    #region Edge cases with noise words

    /// <summary>
    /// Verifies that noise words in column positions are properly ignored.
    /// </summary>
    [Fact]
    public void IgnoresNoiseWordsInColumnPositions()
    {
        var cols = PredicateColumnScanner.Scan("(AND = 1 AND OR = 2)").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that all noise words are properly filtered.
    /// </summary>
    [Fact]
    public void FiltersAllNoiseWords()
    {
        var cols = PredicateColumnScanner.Scan("(NOT = true AND NULL = 'value' AND TRUE = false AND FALSE = true)").ToList();
        Assert.Empty(cols);
    }

    #endregion

    #region PostgreSQL-specific tests

    /// <summary>
    /// Verifies that PostgreSQL quoted identifiers are handled correctly.
    /// </summary>
    [Fact]
    public void HandlesPostgresQuotedIdentifiers()
    {
        var cols = PredicateColumnScanner.Scan("(\"user\".\"status\" = 'open')", isPostgres: true).ToList();
        Assert.Equal(new[] { "\"status\"" }, cols);
    }

    /// <summary>
    /// Verifies that PostgreSQL quoted identifiers without alias are handled correctly.
    /// </summary>
    [Fact]
    public void HandlesPostgresQuotedIdentifiersWithoutAlias()
    {
        var cols = PredicateColumnScanner.Scan("(\"status\" = 'open')", isPostgres: true).ToList();
        Assert.Equal(new[] { "\"status\"" }, cols);
    }

    #endregion

    #region Empty and null handling

    /// <summary>
    /// Verifies that empty strings are handled gracefully.
    /// </summary>
    [Fact]
    public void HandlesEmptyString()
    {
        var cols = PredicateColumnScanner.Scan(string.Empty).ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that whitespace-only strings are handled gracefully.
    /// </summary>
    [Fact]
    public void HandlesWhitespaceOnly()
    {
        var cols = PredicateColumnScanner.Scan("   ").ToList();
        Assert.Empty(cols);
    }

    /// <summary>
    /// Verifies that null strings are handled gracefully.
    /// </summary>
    [Fact]
    public void HandlesNullString()
    {
        var cols = PredicateColumnScanner.Scan(null!).ToList();
        Assert.Empty(cols);
    }

    #endregion

    #region Real-world complex scenarios

    /// <summary>
    /// Verifies a realistic complex predicate from a SQL execution plan.
    /// </summary>
    [Fact]
    public void HandlesRealWorldComplexPredicate()
    {
        var complexPredicate = "((\"Node Type\" = 'Index Scan' AND \"Index Name\" = 'IX_Users_Status_AccountId')\n            AND (\"Actual Rows\" > 1000 AND \"Alias\" = 'u'))";
        var cols = PredicateColumnScanner.Scan(complexPredicate).ToList();
        // Should extract column references from the predicate part
        Assert.Contains("Status", cols);
        Assert.Contains("AccountId", cols);
    }

    /// <summary>
    /// Verifies a realistic complex predicate with multiple conditions.
    /// </summary>
    [Fact]
    public void HandlesRealWorldMultiConditionPredicate()
    {
        var predicate = "(created_date > '2023-01-01'::date\n            AND status IN ('active', 'pending')\n            AND (category_id = 5 OR category_id = 10)\n            AND priority BETWEEN 1 AND 5)";
        var cols = PredicateColumnScanner.Scan(predicate).ToList();
        Assert.Equal(new[] { "created_date", "status", "category_id", "category_id", "priority" }, cols);
    }

    #endregion
}