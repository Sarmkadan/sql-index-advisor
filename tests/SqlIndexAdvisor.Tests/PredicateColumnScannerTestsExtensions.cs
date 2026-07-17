using System;
using System.Collections.Generic;
using System.Linq;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods that make it easier to work with <see cref="PredicateColumnScannerTests"/> in unit tests.
/// </summary>
public static class PredicateColumnScannerTestsExtensions
{
    /// <summary>
    /// Scans the supplied <paramref name="expression"/> for column identifiers using <see cref="PredicateColumnScanner"/>.
    /// </summary>
    /// <param name="_">The test instance (unused, required for extension method syntax).</param>
    /// <param name="expression">The predicate expression to scan.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing the column names found, in the order they appear.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="_"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> is <c>null</c> or empty.</exception>
    public static IReadOnlyList<string> ScanColumns(this PredicateColumnScannerTests _, string expression)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentException.ThrowIfNullOrEmpty(expression);

        // PredicateColumnScanner returns IEnumerable<string>; materialise as a read‑only list.
        return PredicateColumnScanner.Scan(expression).ToList().AsReadOnly();
    }

    /// <summary>
    /// Asserts that the <paramref name="expression"/> contains all of the <paramref name="expected"/> column names.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <param name="expression">The predicate expression to scan.</param>
    /// <param name="expected">Column names that must be present.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="_"/> or <paramref name="expected"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> is <c>null</c> or empty.</exception>
    public static void AssertContainsColumns(this PredicateColumnScannerTests _, string expression, params string[] expected)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentException.ThrowIfNullOrEmpty(expression);
        ArgumentNullException.ThrowIfNull(expected);

        var cols = _.ScanColumns(expression);
        foreach (var col in expected)
        {
            Assert.Contains(col, cols);
        }
    }

    /// <summary>
    /// Asserts that the <paramref name="expression"/> does not contain any of the <paramref name="notExpected"/> column names.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <param name="expression">The predicate expression to scan.</param>
    /// <param name="notExpected">Column names that must be absent.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="_"/> or <paramref name="notExpected"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> is <c>null</c> or empty.</exception>
    public static void AssertNoColumns(this PredicateColumnScannerTests _, string expression, params string[] notExpected)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentException.ThrowIfNullOrEmpty(expression);
        ArgumentNullException.ThrowIfNull(notExpected);

        var cols = _.ScanColumns(expression);
        foreach (var col in notExpected)
        {
            Assert.DoesNotContain(col, cols);
        }
    }
}
