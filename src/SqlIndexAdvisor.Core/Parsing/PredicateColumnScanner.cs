using System.Text.RegularExpressions;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Extracts column names from a Postgres predicate expression string.
/// This is intentionally simple - it grabs the identifier on the left of a
/// comparison operator. It won't understand function calls wrapping a column
/// (e.g. lower(name) = ...), and that's fine: such predicates aren't
/// sargable anyway so skipping them is the correct behavior.
/// </summary>
public static partial class PredicateColumnScanner
{
    // matches:  optional "alias." then identifier, then a comparison operator
    [GeneratedRegex(@"(?:[A-Za-z_][A-Za-z0-9_]*\.)?([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|<>|!=|<=|>=|<|>|~~|IN\b|BETWEEN\b)",
        RegexOptions.IgnoreCase)]
    private static partial Regex PredicateRegex();

    private static readonly HashSet<string> Noise = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "OR", "NOT", "NULL", "TRUE", "FALSE", "ANY", "ALL"
    };

    public static IEnumerable<string> Scan(string expression)
    {
        foreach (Match m in PredicateRegex().Matches(expression))
        {
            var col = m.Groups[1].Value;
            if (Noise.Contains(col)) continue;
            yield return col;
        }
    }
}
