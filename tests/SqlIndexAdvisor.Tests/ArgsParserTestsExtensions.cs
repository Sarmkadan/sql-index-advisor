using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides additional helper methods for <see cref="ArgsParserTests"/>.
/// </summary>
public static class ArgsParserTestsExtensions
{
    /// <summary>
    /// Executes all public instance methods of <see cref="ArgsParserTests"/> whose name starts with <c>Parse_</c>.
    /// </summary>
    /// <param name="test">The test instance on which to invoke the methods.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> containing the names of the methods that completed without throwing an exception.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> RunAllParseTests(this ArgsParserTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var passed = new List<string>();

        // Find all public instance methods that match the naming convention.
        var methods = typeof(ArgsParserTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name.StartsWith("Parse_", StringComparison.Ordinal) && m.GetParameters().Length == 0 && m.ReturnType == typeof(void));

        foreach (var method in methods)
        {
            try
            {
                method.Invoke(test, null);
                passed.Add(method.Name);
            }
            catch
            {
                // Swallow any exception – the method is considered failed.
                // The caller can compare the returned list with the total count to detect failures.
            }
        }

        return passed.AsReadOnly();
    }

    /// <summary>
    /// Determines whether every <c>Parse_*</c> test method on <see cref="ArgsParserTests"/> succeeds.
    /// </summary>
    /// <param name="test">The test instance to evaluate.</param>
    /// <returns><c>true</c> if all <c>Parse_*</c> methods complete without throwing; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static bool AllParseTestsPass(this ArgsParserTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var allMethods = typeof(ArgsParserTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Count(m => m.Name.StartsWith("Parse_", StringComparison.Ordinal) && m.GetParameters().Length == 0 && m.ReturnType == typeof(void));

        var passed = test.RunAllParseTests().Count;

        return passed == allMethods;
    }

    /// <summary>
    /// Returns the list of output formats that are explicitly validated by the <c>Parse_Valid*Format</c> tests.
    /// </summary>
    /// <param name="test">The test instance (unused, but required for the extension method signature).</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of format strings in lower‑case (e.g., <c>text</c>, <c>json</c>, <c>html</c>, <c>csv</c>).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetSupportedFormats(this ArgsParserTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        // Identify methods named like Parse_Valid{Format}Format_ReturnsCorrectFormat
        var formatMethods = typeof(ArgsParserTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name.StartsWith("Parse_Valid", StringComparison.Ordinal) && m.Name.Contains("Format_ReturnsCorrectFormat"));

        var formats = formatMethods
            .Select(m =>
            {
                // Extract the format part between "Parse_Valid" and "Format_ReturnsCorrectFormat"
                var start = "Parse_Valid".Length;
                var end = m.Name.IndexOf("Format_ReturnsCorrectFormat", StringComparison.Ordinal);
                var raw = m.Name[start..end];
                return raw.ToLowerInvariant();
            })
            .Distinct()
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        return formats.AsReadOnly();
    }

    /// <summary>
    /// Retrieves the names of all public instance methods on <see cref="ArgsParserTests"/> that start with <c>Parse_</c>.
    /// </summary>
    /// <param name="test">The test instance (unused, but required for the extension method signature).</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of method names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetParseMethodNames(this ArgsParserTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var names = typeof(ArgsParserTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name.StartsWith("Parse_", StringComparison.Ordinal))
            .Select(m => m.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        return names.AsReadOnly();
    }
}
